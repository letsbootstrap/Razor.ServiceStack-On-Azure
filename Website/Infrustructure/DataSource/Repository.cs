using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Linq.Expressions;
using ServiceStack.OrmLite;
using System.Data;

namespace Website.Infrustructure.DataSource
{
    public interface IRepository
    {
        IList<T> All<T>() where T : class, new();
        IList<T> Find<T>(Expression<Func<T, bool>> whereClause) where T : class, new();
        IList<T> Find<T>(Expression<Func<T, bool>> whereClause, int pageSize, int pageNumber) where T : class, new();
        IList<T> Find<T>(Expression<Func<T, bool>> whereClause, Expression<Func<T, object>> orderByClause, bool isASC, int pageSize, int pageNumber) where T : class, new();
        IList<T> Find<T>(string sql, object parameters = null) where T : class, new();
        int Count<T>(string sql, object parameters = null) where T : new();
        T Single<T>(object id) where T : class, new();
        T Add<T>(T data) where T : class, new();
        void AddMany<T>(IEnumerable<T> list) where T : class, new();
        void Update<T>(T data) where T : class, new();
        void Delete<T>(object key) where T : class, new();
        void DeleteMany<T>(IEnumerable<T> list) where T : class, new();
        void UpdateMany<T>(IEnumerable<T> list) where T : class, new();
    }

    public class Repository : IRepository, IDisposable
    {
        public IDbConnectionFactory DbFactory { get; set; }     // Injected by IOC

        public Repository(IDbConnectionFactory dbFactory)
        {
            DbFactory = dbFactory;
        }

        #region IRepository Members

        public IList<T> All<T>() where T : class, new()
        {
            return DbFactory.Run(repo => repo.Select<T>());
        }

        public IList<T> Find<T>(Expression<Func<T, bool>> whereClause) where T : class, new()
        {
            return DbFactory.Run(repo => repo.Select<T>(whereClause));
        }

        public IList<T> Find<T>(Expression<Func<T, bool>> whereClause, int pageSize, int pageNumber) where T : class, new()
        {
            return DbFactory.Run(repo => repo.Select<T>(t => t.Where(whereClause).Limit(pageSize * pageNumber, pageSize)));
        }

        public IList<T> Find<T>(Expression<Func<T, bool>> whereClause, Expression<Func<T, object>> orderByClause, bool isASC, int pageSize, int pageNumber) where T : class, new()
        {
            // Usage example:
            // Expression<Func<ContentItem, object>> orderByClause;
            // orderByClause = n => n.CreatedDate;

            if (isASC)
                return DbFactory.Run(repo => repo.Select<T>(t => t.Where(whereClause).OrderBy(orderByClause).Limit(pageSize * pageNumber, pageSize)));
            else
                return DbFactory.Run(repo => repo.Select<T>(t => t.Where(whereClause).OrderByDescending(orderByClause).Limit(pageSize * pageNumber, pageSize)));
        }

        public IList<T> Find<T>(string sql, object parameters = null) where T : class, new()
        {
            if (parameters != null)
                return DbFactory.Run(repo => repo.Query<T>(sql, parameters));
            else
                return DbFactory.Run(repo => repo.Query<T>(sql));
        }

        public int Count<T>(string sql, object parameters = null) where T : new()
        {
            if (parameters != null)
                return DbFactory.Run(repo => repo.QueryScalar<int>(sql, parameters));
            else
                return DbFactory.Run(repo => repo.QueryScalar<int>(sql));
        }

        public T Single<T>(object key) where T : class, new()
        {
            return DbFactory.Run(repo => repo.GetById<T>(key));
        }

        public T Add<T>(T data) where T : class, new()
        {
            T obj;
            using (var db = DbFactory.Open())
            {
                db.Insert<T>(data);
                var id = db.GetLastInsertId();
                obj = db.GetById<T>(id);
            }

            return obj;
        }

        public void Update<T>(T data) where T : class, new()
        {
            DbFactory.Run(repo => repo.Update<T>(data));
        }

        public void Delete<T>(object key) where T : class, new()
        {
            DbFactory.Run(repo => repo.DeleteById<T>(key));
        }

        public void DeleteMany<T>(IEnumerable<T> items) where T : class, new()
        {
            DbFactory.Run(repo => repo.DeleteAll<T>(items));
        }

        public void UpdateMany<T>(IEnumerable<T> items) where T : class, new()
        {
            DbFactory.Run(repo => repo.UpdateAll<T>(items));
        }

        public void AddMany<T>(IEnumerable<T> items) where T : class, new()
        {
            DbFactory.Run(repo => repo.InsertAll<T>(items));
        }

        #endregion

        public void Dispose()
        {
            DbFactory = null;
        }
    }
}