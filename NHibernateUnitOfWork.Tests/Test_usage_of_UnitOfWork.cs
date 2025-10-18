using System;
using System.Reflection;
using NHibernate.Tool.hbm2ddl;
using NUnit.Framework;

namespace NHibernateUnitOfWork.Tests
{
    [TestFixture]
    public class TestUsageOfUnitOfWork
    {
        [SetUp]
        public void SetupContext()
        {
            // Inicializa a configuração do NHibernate e cria o schema no banco de teste
            UnitOfWork.Configuration.AddAssembly(Assembly.GetExecutingAssembly());
            var schemaExport = new SchemaExport(UnitOfWork.Configuration);
            schemaExport.Execute(useStdOut: false, execute: true, justDrop: false);
        }

        [Test]
        public void CanAddNewInstanceOfEntityToDatabase()
        {
            using (UnitOfWork.Start())
            {
                var person = new Person
                {
                    Name = "John Doe",
                    Birthdate = new DateTime(1915, 12, 15)
                };

                UnitOfWork.CurrentSession.Save(person);
                UnitOfWork.Current.TransactionalFlush();
            }

            Assert.Pass("Entity successfully added to database.");
        }
    }

    public class Person
    {
        public virtual Guid Id { get; set; }
        public virtual string Name { get; set; }
        public virtual DateTime Birthdate { get; set; }
    }
}
