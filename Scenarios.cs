using System;
using Autofac;
using Autofac.Core;
using AutofacRegistration.Tests.Conventions;
using NUnit.Framework;

namespace AutofacRegistration.Tests
{
    public interface IBarService
    {
    }

    public interface IFooService
    {
    }

    public interface IBoomViewModel
    {
    }

    public class BarService : IBarService
    {
    }

    public class FooService : IFooService
    {
        private readonly IBarService _barService;

        public FooService(IBarService barService)
        {
            _barService = barService;
        }
    }

    public class BoomViewModel : IBoomViewModel
    {
        private readonly IBarService _barService;

        public delegate BoomViewModel Factory(Guid id);

        public BoomViewModel(Guid id, IBarService barService)
        {
            _barService = barService;
        }
    }

    public class TestModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<FooService>()
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();

            builder.RegisterType<BarService>()
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();

            builder.RegisterType<BoomViewModel>()
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();
        }
    }

    [TestFixture]
    public class WhenTypesAreRegister
    {
        [SetUp]
        public void SetUp()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new TestModule());

            _container = builder.Build();
        }

        [TearDown]
        public void TearDown()
        {
            _container.Dispose();
        }

        private IContainer _container;

        [Test]
        public void ShouldResolveBoomToNotThrowException()
        {   
            using (var scope = _container.BeginLifetimeScope())
            {
                _container.Resolve<IBoomViewModel>(new NamedParameter("id", Guid.NewGuid()));
            }
        }

        [Test]
        public void ShouldResolveBoomToThrowException()
        {
            Assert.Catch<DependencyResolutionException>(() =>
            {
                using (var scope = _container.BeginLifetimeScope())
                {
                    _container.Resolve<IBoomViewModel>();
                }
            });
        }

        [Test]
        public void ShouldResolveFoo()
        {
            using (var scope = _container.BeginLifetimeScope())
            {
                _container.Resolve<IFooService>();
            }
        }

        [Test]
        public void ShouldResolveAllTypesInContainer()
        {
            var assertion = new AutofacContainerAssertion(Filter, IsKnownOffender);
            assertion.Verify(_container);
        }
        
        private bool Filter(Type serviceType)
        {
            return true;
        }

        private bool IsKnownOffender(Type serviceType)
        {
            return false;
        }
    }
}