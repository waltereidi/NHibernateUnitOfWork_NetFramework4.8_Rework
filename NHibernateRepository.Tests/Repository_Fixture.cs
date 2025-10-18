using System;
using System.Linq; 
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Criterion;
using NHibernate.Tool.hbm2ddl;
using NHibernateUnitOfWork;
using NUnit.Framework;

namespace NHibernateRepository.Tests
{
    [TestFixture]
    public class Repository_Fixture
    {
        private IUnitOfWork _unitOfWork;
        private Product _product;
        private IRepository<Product> _repository;

        private Configuration Configuration => UnitOfWork.Configuration;

        [OneTimeSetUp]
        public void TestFixtureSetUp()
        {
            Configuration.AddAssembly(typeof(Product).Assembly);
        }

        [SetUp]
        public void SetupContext()
        {
            
            _unitOfWork = UnitOfWork.Start();
            //Executes migration upon start, create tables
            var schemaExport = new SchemaExport(Configuration);
            schemaExport.Execute(
                useStdOut: false,
                execute: true,
                justDrop: false,
                connection: UnitOfWork.CurrentSession.Connection,
                exportOutput: null
            );

            CreateInitialData();
            _repository = new Repository<Product>();
        }

        /// <summary>
        /// Insert a product in db
        /// </summary>
        private void CreateInitialData()
        {

            _product = new Product { Name = "Apples" };
            UnitOfWork.CurrentSession.Save(_product);
            UnitOfWork.Current.TransactionalFlush();
            UnitOfWork.CurrentSession.Clear();
        }

        [TearDown]
        public void TearDownContext()
        {
            _unitOfWork?.Dispose();
        }
        /// <summary>
        /// Get registry from db , using GUID from setup
        /// </summary>
        [Test]
        public void Can_get_product()
        {
            var fromDb = _repository.Get(_product.Id);
            Assert.IsNotNull(fromDb);
            Assert.AreEqual(_product.Name, fromDb.Name);
        }

        [Test]
        public void Can_find_all()
        {
            var products = _repository.FindAll(Order.Asc("Name")).ToList(); 
            Assert.AreEqual(1, products.Count);
            Assert.AreEqual("Apples", products[0].Name);
        }

        [Test]
        public void Can_report_product()
        {
            var projectionList = Projections.ProjectionList()
                .Add(Projections.Property("Id"), "Id")
                .Add(Projections.Property("Name"), "Name");

            var products = _repository.ReportAll<ProductDTO>(projectionList).ToList(); 

            Assert.AreEqual(1, products.Count);
            Assert.AreEqual("Apples", products[0].Name);
        }
    }
}
