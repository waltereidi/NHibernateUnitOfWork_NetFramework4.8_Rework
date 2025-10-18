using System;
using NHibernate;
using NUnit.Framework;

namespace NHibernateUnitOfWork.Tests
{
    [TestFixture]
    public class UnitOfWorkFactory_Fixture
    {
        private IUnitOfWorkFactory _factory;

        [SetUp]
        public void SetupContext()
        {
            // Cria a instância privada da UnitOfWorkFactory
            _factory = (IUnitOfWorkFactory)Activator.CreateInstance(typeof(UnitOfWorkFactory), nonPublic: true);
        }

        [Test]
        public void Can_create_unit_of_work()
        {
            IUnitOfWork implementor = _factory.Create();

            Assert.IsNotNull(implementor, "UnitOfWorkFactory.Create() retornou nulo.");
            Assert.IsNotNull(_factory.CurrentSession, "A sessão atual não foi inicializada.");
            Assert.AreEqual(FlushMode.Commit, _factory.CurrentSession.FlushMode, "O FlushMode deveria ser Commit.");
        }

        [Test]
        public void Can_configure_NHibernate()
        {
            var configuration = _factory.Configuration;

            Assert.IsNotNull(configuration, "A configuração do NHibernate não foi inicializada.");

            // Os valores abaixo podem variar conforme o hibernate.cfg.xml usado
            Assert.AreEqual("NHibernate.Connection.DriverConnectionProvider",
                configuration.Properties["connection.provider"]);

            Assert.AreEqual("NHibernate.Dialect.MsSql2005Dialect",
                configuration.Properties["dialect"]);

            Assert.AreEqual("NHibernate.Driver.SqlClientDriver",
                configuration.Properties["connection.driver_class"]);

            Assert.AreEqual("Server=(local);Database=Test;Integrated Security=SSPI;",
                configuration.Properties["connection.connection_string"]);
        }

        [Test]
        public void Can_create_and_access_session_factory()
        {
            var sessionFactory = _factory.SessionFactory;

            Assert.IsNotNull(sessionFactory, "O SessionFactory não foi criado corretamente.");

            // A partir do NHibernate 5, a Dialect é acessível via configuração, não diretamente da factory
            var dialect = _factory.Configuration.GetProperty("dialect");
            Assert.AreEqual("NHibernate.Dialect.MsSql2005Dialect", dialect);
        }

        [Test]
        public void Accessing_CurrentSession_when_no_session_open_throws()
        {
            // Fecha todas as sessões abertas, se houver
            try
            {
                var session = _factory.CurrentSession;
                Assert.Fail("Esperava InvalidOperationException ao acessar CurrentSession sem sessão aberta.");
            }
            catch (InvalidOperationException)
            {
                Assert.Pass("Lançou exceção esperada ao acessar CurrentSession sem sessão ativa.");
            }
        }
    }
}
