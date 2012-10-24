using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Xml.Serialization;

namespace Website.Infrustructure.DataSource
{
    public interface IPoco
    {
    }

    public class Poco : IPoco
    {
        public Poco()
        {
            // Can't use Ioc as we are inheriting this as a concrete class and not as an interface:
            repository = AppHost.Instance.Container.TryResolve<IRepository>();
        }

        [XmlIgnore]
        [ScriptIgnore]
        [IgnoreAttribute]
        private IRepository repository { get; set; }     // Injected by IOC

        [AutoIncrement]                                 // Creates Auto primary key
        public int Id { get; set; }

        public IRepository Repository()
        {
            return repository;
        }
    }
}