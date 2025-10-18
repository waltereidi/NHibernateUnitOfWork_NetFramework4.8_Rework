using System;
using System.Reflection;
using NHibernate;
using NUnit.Framework;
using Rhino.Mocks;

namespace NHibernateUnitOfWork.Tests
{
    [TestFixture]
    public class UnitOfWork_Fixture
    {
        private readonly MockRepository _mocks = new MockRepository();

        [Test]
        public void Can_Start_UnitOfWork()
        {
            var factory = _mocks.DynamicMock<IUnitOfWorkFactory>();
            var unitOfWork = _mocks.DynamicMock<IUnitOfWork>();

            // força o uso do factory de mock via reflection
            var fieldInfo = typeof(UnitOfWork).GetField("_unitOfWorkFactory",
                BindingFlags.Static | BindingFlags.NonPublic);
            fieldInfo.SetValue(null, factory);

            using (_mocks.Record())
            {
                Expect.Call(factory.Create()).Return(unitOfWork);
            }

            using (_mocks.Playback())
            {
                var uow = UnitOfWork.Start();
                Assert.NotNull(uow);
            }
        }
    }

    [TestFixture]
    public class UnitOfWork_With_Factory_Fixture
    {
        private readonly MockRepository _mocks = new MockRepository();
        private IUnitOfWorkFactory _factory;
        private IUnitOfWork _unitOfWork;
        private ISession _session;

        [OneTimeSetUp]
        public void TestFixtureSetUp()
        {
            ResetUnitOfWork();
        }

        [SetUp]
        public void SetupContext()
        {
            _factory = _mocks.DynamicMock<IUnitOfWorkFactory>();
            _unitOfWork = _mocks.DynamicMock<IUnitOfWork>();
            _session = _mocks.DynamicMock<ISession>();

            InstrumentUnitOfWork();

            _mocks.BackToRecordAll();
            SetupResult.For(_factory.Create()).Return(_unitOfWork);
            SetupResult.For(_factory.CurrentSession).Return(_session);
            _mocks.ReplayAll();
        }

        [TearDown]
        public void TearDownContext()
        {
            _mocks.VerifyAll();
            ResetUnitOfWork();
        }

        private void InstrumentUnitOfWork()
        {
            var fieldInfo = typeof(UnitOfWork).GetField("_unitOfWorkFactory",
                BindingFlags.Static | BindingFlags.NonPublic);
            fieldInfo.SetValue(null, _factory);
        }

        private void ResetUnitOfWork()
        {
            var propertyInfo = typeof(UnitOfWork).GetProperty("CurrentUnitOfWork",
                BindingFlags.Static | BindingFlags.NonPublic);
            propertyInfo?.SetValue(null, null, null);
        }

        [Test]
        public void Can_Start_and_Dispose_UnitOfWork()
        {
            var uow = UnitOfWork.Start();
            Assert.NotNull(uow);
            uow.Dispose();
        }

        [Test]
        public void Can_access_current_unit_of_work()
        {
            var uow = UnitOfWork.Start();
            var current = UnitOfWork.Current;
            Assert.NotNull(current);
            uow.Dispose();
        }

        [Test]
        public void Accessing_Current_UnitOfWork_if_not_started_throws()
        {
            Assert.Throws<InvalidOperationException>(() => { var _ = UnitOfWork.Current; });
        }

        [Test]
        public void Starting_UnitOfWork_if_already_started_throws()
        {
            UnitOfWork.Start();
            Assert.Throws<InvalidOperationException>(() => UnitOfWork.Start());
        }

        [Test]
        public void Can_test_if_UnitOfWork_Is_Started()
        {
            Assert.IsFalse(UnitOfWork.IsStarted);
            var uow = UnitOfWork.Start();
            Assert.IsTrue(UnitOfWork.IsStarted);
            uow.Dispose();
        }

        [Test]
        public void Can_get_valid_current_session_if_UoW_is_started()
        {
            using (UnitOfWork.Start())
            {
                ISession session = UnitOfWork.CurrentSession;
                Assert.NotNull(session);
            }
        }

        [Test]
        public void Get_current_session_if_UoW_is_not_started_throws()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                var _ = UnitOfWork.CurrentSession;
            });
        }
    }
}
