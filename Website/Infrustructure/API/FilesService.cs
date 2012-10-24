using ServiceStack.Common.Web;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.Auth;
using SquishIt.Framework;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using Website.Infrustructure.Manager;

namespace Website.Infrustructure.API
{
    [Route("/files")]
    [Route("/files/{Reference}")]
    public class Files : IRequiresRequestStream
    {
        public string Reference { get; set; }
        public Stream RequestStream { get; set; }
    }
    public class FileUploadVM
    {
        public int FileSize { get; set; }
        public string Filename { get; set; }
        public string ContentType { get; set; }
        public byte[] Contents { get; set; }
    }

    [Route("/files/js/codebase/{Version}")]
    public class FilesJs
    {
        public string Version { get; set; }
        public string Id { get; set; }
    }
    [Route("/files/css/codebase/{Version}")]
    public class FilesCss
    {
        public string Version { get; set; }
        public string Id { get; set; }
    }

    public class FilesResponse
    {
        public string Url { get; set; }
    }

    [ClientCanSwapTemplates]
    public class FilesService : Service
    {
        public IStorageManager StorageManager { get; set; }

        public object Post(Files request)
        {
            IAuthSession session = this.Request.GetSession();
            if (session.IsAuthenticated)
            {
                string virtualPath = "";
                request.Reference = base.RequestContext.PathInfo.Split('/')[2].ToLower();

                FileUploadVM file = RetrieveFileFromRequest(base.RequestContext, request.RequestStream);
                file.Filename = request.Reference;

                Stream stream = new MemoryStream(file.Contents);
                if (request.Reference.StartsWith("asset-"))
                    virtualPath = SaveCloudFile(int.Parse(session.UserAuthId), file);
                else
                    return new HttpResult("Invalid file upload.", HttpStatusCode.BadRequest);

                return new HttpResult(new FilesResponse { Url = virtualPath }, "application/json", HttpStatusCode.OK);
            }
            else
            {
                return new HttpResult("Please Sign in first.", HttpStatusCode.Unauthorized);
            }
        }

        public object Get(FilesJs request)
        {
            request.Id = base.Request.QueryString["r"].ToString();

            // Set max-age to a year from now
            Response.AddHeader("Cache-Control", "max-age=" + TimeSpan.FromDays(365));
            return new HttpResult(Bundle.JavaScript().RenderCached("codebase"), "text/javascript");
        }

        public object Get(FilesCss request)
        {
            request.Id = base.Request.QueryString["r"].ToString();

            // Set max-age to a year from now
            Response.AddHeader("Cache-Control", "max-age=" + TimeSpan.FromDays(365));
            return new HttpResult(Bundle.Css().RenderCached("stylebase"), "text/css");
        }

        #region Helper Functions

        private FileUploadVM RetrieveFileFromRequest(IRequestContext request, Stream requestStream)
        {
            string filename = null;
            string fileType = null;
            byte[] fileContents = null;

            if (request.Files.Count() > 0)
            { //we are uploading the old way
                var file = Request.Files[0];
                fileContents = new byte[file.ContentLength];
                file.InputStream.Read(fileContents, 0, (int)file.ContentLength);
                fileType = file.ContentType;
                filename = file.FileName;
            }
            else if (Request.ContentLength > 0)
            {
                // Using FileAPI the content is in Request.InputStream!!!!
                fileContents = new byte[Request.ContentLength];
                requestStream.Read(fileContents, 0, (int)Request.ContentLength);
                filename = Request.Headers["X-File-Name"];
                fileType = Request.Headers["X-File-Type"];
                if (fileType == null)
                    fileType = Request.Headers["X-Mime-Type"];
            }

            return new FileUploadVM()
            {
                Filename = filename,
                ContentType = fileType,
                FileSize = fileContents != null ? fileContents.Length : 0,
                Contents = fileContents
            };
        }

        private string ResizeAndSaveCloudImage(int authUserId, string fileName, Stream imageBuffer, int maxSideSize, bool makeItSquare)
        {
            int newWidth;
            int newHeight;
            Image image = Image.FromStream(imageBuffer);
            int oldWidth = image.Width;
            int oldHeight = image.Height;
            Bitmap newImage;
            if (makeItSquare)
            {
                int smallerSide = oldWidth >= oldHeight ? oldHeight : oldWidth;
                double coeficient = maxSideSize / (double)smallerSide;
                newWidth = Convert.ToInt32(coeficient * oldWidth);
                newHeight = Convert.ToInt32(coeficient * oldHeight);
                Bitmap tempImage = new Bitmap(image, newWidth, newHeight);
                int cropX = (newWidth - maxSideSize) / 2;
                int cropY = (newHeight - maxSideSize) / 2;
                newImage = new Bitmap(maxSideSize, maxSideSize);
                Graphics tempGraphic = Graphics.FromImage(newImage);
                tempGraphic.SmoothingMode = SmoothingMode.AntiAlias;
                tempGraphic.InterpolationMode = InterpolationMode.HighQualityBicubic;
                tempGraphic.PixelOffsetMode = PixelOffsetMode.HighQuality;
                tempGraphic.DrawImage(tempImage, new Rectangle(0, 0, maxSideSize, maxSideSize), cropX, cropY, maxSideSize, maxSideSize, GraphicsUnit.Pixel);
            }
            else
            {
                int maxSide = oldWidth >= oldHeight ? oldWidth : oldHeight;

                if (maxSide > maxSideSize)
                {
                    double coeficient = maxSideSize / (double)maxSide;
                    newWidth = Convert.ToInt32(coeficient * oldWidth);
                    newHeight = Convert.ToInt32(coeficient * oldHeight);
                }
                else
                {
                    newWidth = oldWidth;
                    newHeight = oldHeight;
                }
                newImage = new Bitmap(image, newWidth, newHeight);
            }
            // Save locally to file system:
            //newImage.Save(savePath + fileName + ".jpg", ImageFormat.Jpeg);

            // Save to Azure cloud storage:
            ImageConverter converter = new ImageConverter();
            var img = StorageManager.SaveImage(
                authUserId,
                "user-" + authUserId.ToString(),
                Guid.NewGuid().ToString(),
                fileName + ".jpg",
                fileName + ".jpg",
                "image/jpg",
                (byte[])converter.ConvertTo(newImage, typeof(byte[]))
              );

            image.Dispose();
            newImage.Dispose();

            return img.Uri.ToString();
        }

        private string SaveCloudFile(int authUserId, FileUploadVM file)
        {
            var image = StorageManager.SaveImage(
                authUserId,
                "user-" + authUserId.ToString(),
                Guid.NewGuid().ToString(),
                file.Filename,
                file.Filename,
                file.ContentType,
                file.Contents
              );

            return image.Uri.ToString();
        }

        #endregion
    }
}