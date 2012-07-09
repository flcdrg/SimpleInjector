﻿namespace SimpleInjector.Extensions.Tests.Unit
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Linq.Expressions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// This set of tests test whether individual items of registered collections are correctly decorated.
    /// </summary>
    [TestClass]
    public class DecoratorExtensionsCollectionTests
    {
        public interface INonGenericService
        {
            void DoSomething();
        }

        public interface ICommandHandler<TCommand>
        {
            void Handle(TCommand command);
        }

        [TestMethod]
        public void GetAllInstances_TypeDecorated1_ReturnsCollectionWithDecorators()
        {
            // Arrange
            var container = new Container();

            // Use the RegisterAll(Type, Type[]) overload.
            container.RegisterAll(typeof(ICommandHandler<RealCommand>), new[] { typeof(RealCommandCommandHandler) });

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(RealCommandCommandHandlerDecorator));

            // Act
            var handlers = container.GetAllInstances<ICommandHandler<RealCommand>>();

            var handler = handlers.Single();

            // Assert
            Assert.IsInstanceOfType(handler, typeof(RealCommandCommandHandlerDecorator));

            Assert.IsInstanceOfType(((RealCommandCommandHandlerDecorator)handler).Decorated,
                typeof(RealCommandCommandHandler));
        }

        [TestMethod]
        public void GetAllInstances_TypeDecorated2_ReturnsCollectionWithDecorators()
        {
            // Arrange
            var container = new Container();

            var expectedSingletonHandler = new RealCommandCommandHandler();

            // Use the RegisterAll<T>(T[]) overload.
            container.RegisterAll<ICommandHandler<RealCommand>>(expectedSingletonHandler);

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(RealCommandCommandHandlerDecorator));

            // Act
            var handlers = container.GetAllInstances<ICommandHandler<RealCommand>>();

            var decorator = handlers.Single();

            // Assert
            Assert.IsInstanceOfType(decorator, typeof(RealCommandCommandHandlerDecorator));

            Assert.IsTrue(object.ReferenceEquals(expectedSingletonHandler,
                ((RealCommandCommandHandlerDecorator)decorator).Decorated));
        }

        [TestMethod]
        public void GetAllInstances_TypeDecorated3_ReturnsCollectionWithDecorators()
        {
            // Arrange
            var container = new Container();

            var expectedSingletonHandler = new RealCommandCommandHandler();

            // Use the RegisterAll(Type, IEnumerable) overload.
            container.RegisterAll(typeof(ICommandHandler<RealCommand>), new[] { expectedSingletonHandler });

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(RealCommandCommandHandlerDecorator));

            // Act
            var handlers = container.GetAllInstances<ICommandHandler<RealCommand>>();

            var decorator = handlers.Single();

            // Assert
            Assert.IsInstanceOfType(decorator, typeof(RealCommandCommandHandlerDecorator));

            Assert.IsTrue(object.ReferenceEquals(expectedSingletonHandler,
                ((RealCommandCommandHandlerDecorator)decorator).Decorated));
        }

        [TestMethod]
        public void GetAllInstances_TypeDecorated4_ReturnsCollectionWithDecorators()
        {
            // Arrange
            var container = new Container();

            var expectedSingletonHandler = new RealCommandCommandHandler();

            // Use the RegisterSingle<IEnumerable<T>> method.
            container.RegisterSingle<IEnumerable<ICommandHandler<RealCommand>>>(new[] { expectedSingletonHandler });

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(RealCommandCommandHandlerDecorator));

            // Act
            var handlers = container.GetAllInstances<ICommandHandler<RealCommand>>();

            var decorator = handlers.Single();

            // Assert
            Assert.IsInstanceOfType(decorator, typeof(RealCommandCommandHandlerDecorator));

            Assert.IsTrue(object.ReferenceEquals(expectedSingletonHandler,
                ((RealCommandCommandHandlerDecorator)decorator).Decorated));
        }

        [TestMethod]
        public void GetAllInstances_RegistrationThatAlwaysReturnsANewCollectionAndDecorator_ReturnsTransientInstances()
        {
            // Arrange
            var container = new Container();

            // Use the Register<T>(Func<T>) method. This is a strange (not adviced), but valid way of 
            // registering collections.
            container.Register<IEnumerable<ICommandHandler<RealCommand>>>(
                () => new[] { new RealCommandCommandHandler() });

            container.RegisterDecorator(
                typeof(ICommandHandler<>),
                typeof(RealCommandCommandHandlerDecorator));

            // Act
            var handlers1 = container.GetAllInstances<ICommandHandler<RealCommand>>();
            var handlers2 = container.GetAllInstances<ICommandHandler<RealCommand>>();

            var decorator1 = (RealCommandCommandHandlerDecorator)handlers1.Single();
            var decorator2 = (RealCommandCommandHandlerDecorator)handlers2.Single();

            // Assert
            bool isTransient = !object.ReferenceEquals(decorator1.Decorated, decorator2.Decorated);

            Assert.IsTrue(isTransient,
                "Since the registration returns a new collection with new instances, the decorators are " +
                "expected to be wrapped around those new instances, and not caching the collection that " +
                "is returned first.");
        }

        [TestMethod]
        public void GetAllInstances_RegistrationThatAlwaysReturnsANewCollectionAndSingletonDecorator_ReturnsSingletons()
        {
            // Arrange
            var container = new Container();

            // Use the Register<T>(Func<T>) method. This is a strange (not adviced), but valid way of 
            // registering collections.
            container.Register<IEnumerable<ICommandHandler<RealCommand>>>(
                () => new[] { new RealCommandCommandHandler() });

            container.RegisterSingleDecorator(
                typeof(ICommandHandler<>),
                typeof(RealCommandCommandHandlerDecorator));

            // Act
            var handlers1 = container.GetAllInstances<ICommandHandler<RealCommand>>();
            var handlers2 = container.GetAllInstances<ICommandHandler<RealCommand>>();

            var decorator1 = (RealCommandCommandHandlerDecorator)handlers1.Single();
            var decorator2 = (RealCommandCommandHandlerDecorator)handlers2.Single();

            // Assert
            bool isSingleton = object.ReferenceEquals(decorator1, decorator2);

            Assert.IsTrue(isSingleton,
                "Since the decorator is registered as singleton, is should be returned as singleton, no " +
                "matter how the collection is registered (as Register<T>(Func<T>) in this case).");
        }

        [TestMethod]
        public void GetAllInstances_TypeDecoratedWithMultipleDecorators_ReturnsCollectionWithDecorators()
        {
            // Arrange
            var container = new Container();

            container.RegisterAll(typeof(ICommandHandler<RealCommand>), new[] { typeof(RealCommandCommandHandler) });

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(TransactionalCommandHandlerDecorator<>));

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(RealCommandCommandHandlerDecorator));

            // Act
            var handlers = container.GetAllInstances<ICommandHandler<RealCommand>>();

            var handler = handlers.Single();

            // Assert
            Assert.IsInstanceOfType(handler, typeof(RealCommandCommandHandlerDecorator));

            Assert.IsInstanceOfType(((RealCommandCommandHandlerDecorator)handler).Decorated,
                typeof(TransactionalCommandHandlerDecorator<RealCommand>));
        }

        [TestMethod]
        public void GetAllInstances_TypeDecoratedWithTransientDecorator_ReturnsANewInstanceEveryTime()
        {
            // Arrange
            var container = new Container();

            container.RegisterAll(typeof(ICommandHandler<RealCommand>), new[] { typeof(RealCommandCommandHandler) });

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(RealCommandCommandHandlerDecorator));

            // Act
            IEnumerable<ICommandHandler<RealCommand>> handlers =
                container.GetAllInstances<ICommandHandler<RealCommand>>();

            var handler1 = handlers.Single();
            var handler2 = handlers.Single();

            // Assert
            Assert.IsInstanceOfType(handler1, typeof(RealCommandCommandHandlerDecorator));

            Assert.IsFalse(object.ReferenceEquals(handler1, handler2),
                "Since the decorator is registered as transient, every time the collection is iterated, " +
                "a new instance should be created.");
        }

        [TestMethod]
        public void GetAllInstances_TypeDecoratedWithFuncDecorator1_InjectsADelegateThatCanCreateThatInstance()
        {
            // Arrange
            var container = new Container();

            // Use the RegisterAll(Type, Type[]) overload.
            container.RegisterAll(typeof(ICommandHandler<RealCommand>), new[] 
            { 
                typeof(RealCommandCommandHandler),
            });

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(AsyncCommandHandlerProxy<>));

            // Act
            var handlers = container.GetAllInstances<ICommandHandler<RealCommand>>();

            var decorator = (AsyncCommandHandlerProxy<RealCommand>)handlers.Single();

            var handler = decorator.DecorateeFactory();

            // Assert
            Assert.IsInstanceOfType(handler, typeof(RealCommandCommandHandler));
        }

        [TestMethod]
        public void GetAllInstances_TypeDecoratedWithFuncDecorator2_InjectsADelegateThatCanCreateThatInstance()
        {
            // Arrange
            var container = new Container();

            var expectedSingletonHandler = new RealCommandCommandHandler();

            // Use the RegisterAll<T>(T[]) overload.
            container.RegisterAll<ICommandHandler<RealCommand>>(expectedSingletonHandler);

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(AsyncCommandHandlerProxy<>));

            // Act
            var handlers = container.GetAllInstances<ICommandHandler<RealCommand>>();

            var decorator = (AsyncCommandHandlerProxy<RealCommand>)handlers.Single();

            var handler = decorator.DecorateeFactory();

            // Assert
            Assert.IsInstanceOfType(handler, typeof(RealCommandCommandHandler));
        }

        [TestMethod]
        public void GetAllInstances_CollectionManuallyRegisteredAndFuncDecoraterRegistered1_ThrowsExpectedException()
        {
            // Arrange
            var container = new Container();

            var expectedSingletonHandler = new RealCommandCommandHandler();

            // Use the RegisterAll(Type, IEnumerable) overload.
            container.RegisterAll(typeof(ICommandHandler<RealCommand>), new[] { expectedSingletonHandler });

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(AsyncCommandHandlerProxy<>));

            try
            {
                // Act
                container.GetAllInstances<ICommandHandler<RealCommand>>();

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (ActivationException ex)
            {
                Assert_ExceptionContainsInfoAboutManualCollectionRegistrationMixedDecoratorsThatTakeAFunc(ex);
            }
        }

        [TestMethod]
        public void GetAllInstances_CollectionManuallyRegisteredAndFuncDecoraterRegistered2_ThrowsExpectedException()
        {
            // Arrange
            var container = new Container();

            var expectedSingletonHandler = new RealCommandCommandHandler();

            // Use the RegisterSingle<IEnumerable<T>> method
            container.RegisterSingle<IEnumerable<ICommandHandler<RealCommand>>>(new[] { expectedSingletonHandler });

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(AsyncCommandHandlerProxy<>));

            try
            {
                // Act
                container.GetAllInstances<ICommandHandler<RealCommand>>();

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (ActivationException ex)
            {
                Assert_ExceptionContainsInfoAboutManualCollectionRegistrationMixedDecoratorsThatTakeAFunc(ex);
            }
        }

        [TestMethod]
        public void GetAllInstances_CollectionManuallyRegisteredAndFuncDecoraterRegistered4_ThrowsExpectedException()
        {
            // Arrange
            var container = new Container();

            var expectedSingletonHandler = new RealCommandCommandHandler();

            // Use the Register<T>(Func<T>) method. This is a strange (but legal) way of registering a service,
            // but will not work with a Func-Decorator.
            container.Register<IEnumerable<ICommandHandler<RealCommand>>>(() => new[] { expectedSingletonHandler });

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(AsyncCommandHandlerProxy<>));

            try
            {
                // Act
                container.GetAllInstances<ICommandHandler<RealCommand>>();

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (ActivationException ex)
            {
                Assert_ExceptionContainsInfoAboutManualCollectionRegistrationMixedDecoratorsThatTakeAFunc(ex);
            }
        }

        [TestMethod]
        public void GetAllInstances_TypeDecoratedWithFuncDecorator_InjectsADelegateThatReturnsATransientInstance()
        {
            // Arrange
            var container = new Container();

            container.RegisterAll(typeof(ICommandHandler<RealCommand>), new[] 
            { 
                typeof(RealCommandCommandHandler),
            });

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(AsyncCommandHandlerProxy<>));

            // Act
            var handlers = container.GetAllInstances<ICommandHandler<RealCommand>>();

            var decorator = (AsyncCommandHandlerProxy<RealCommand>)handlers.Single();

            var handler1 = decorator.DecorateeFactory();
            var handler2 = decorator.DecorateeFactory();

            // Assert
            Assert.IsFalse(object.ReferenceEquals(handler1, handler2), "The injected Func<T> should create " +
                "a transient, sine that's how the StubCommandHandler is registered.");
        }

        [TestMethod]
        public void GetAllInstances_DecoratorDecoratedWithFuncDecorator_InjectsADelegateThatCanCreateThatDecorator()
        {
            // Arrange
            var container = new Container();

            container.RegisterAll(typeof(ICommandHandler<RealCommand>), new[] 
            { 
                typeof(RealCommandCommandHandler),
            });

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(TransactionalCommandHandlerDecorator<>));
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(AsyncCommandHandlerProxy<>));

            // Act
            var handlers = container.GetAllInstances<ICommandHandler<RealCommand>>();

            var funcDecorator = (AsyncCommandHandlerProxy<RealCommand>)handlers.Single();

            var decorator = funcDecorator.DecorateeFactory();

            // Assert
            Assert.IsInstanceOfType(decorator, typeof(TransactionalCommandHandlerDecorator<RealCommand>));

            var handler = ((TransactionalCommandHandlerDecorator<RealCommand>)decorator).Decorated;

            Assert.IsInstanceOfType(handler, typeof(RealCommandCommandHandler));
        }

        [TestMethod]
        public void GetAllInstances_NestedFuncDecorators_GetInjectedAsExpected()
        {
            // Arrange
            var container = new Container();

            container.RegisterAll(typeof(ICommandHandler<RealCommand>), new[] 
            { 
                typeof(RealCommandCommandHandler),
            });

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(LifetimeScopeCommandHandlerProxy<>));
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(AsyncCommandHandlerProxy<>));

            // Act
            var handlers = container.GetAllInstances<ICommandHandler<RealCommand>>();

            var asyncDecorator = (AsyncCommandHandlerProxy<RealCommand>)handlers.Single();

            var scopeDecorator = (LifetimeScopeCommandHandlerProxy<RealCommand>)asyncDecorator.DecorateeFactory();

            var handler = scopeDecorator.DecorateeFactory();

            // Assert
            Assert.IsInstanceOfType(handler, typeof(RealCommandCommandHandler));
        }

        [TestMethod]
        public void GetAllInstances_FuncDecoratorDecoratedByANormalDecorator_GetInjectedAsExpected()
        {
            // Arrange
            var container = new Container();

            container.RegisterAll(typeof(ICommandHandler<RealCommand>), new[] 
            { 
                typeof(RealCommandCommandHandler),
            });

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(AsyncCommandHandlerProxy<>));
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(TransactionalCommandHandlerDecorator<>));

            // Act
            var handlers = container.GetAllInstances<ICommandHandler<RealCommand>>();

            var decorator = (TransactionalCommandHandlerDecorator<RealCommand>)handlers.Single();

            // Assert
            Assert.IsInstanceOfType(decorator.Decorated, typeof(AsyncCommandHandlerProxy<RealCommand>));
        }

        [TestMethod]
        public void GetAllInstances_TypeRegisteredWithRegisterSingleDecorator_AlwaysReturnsTheSameInstance()
        {
            // Arrange
            var container = new Container();

            // Register transient service
            // This is not good practice, since we register a singleton decorator, but just for testing.
            container.RegisterAll<INonGenericService>(typeof(RealNonGenericService));

            container.RegisterSingleDecorator(typeof(INonGenericService), typeof(NonGenericServiceDecorator));

            // Act
            IEnumerable<INonGenericService> services = container.GetAllInstances<INonGenericService>();

            var decorator1 = services.Single();
            var decorator2 = services.Single();

            // Assert
            Assert.IsInstanceOfType(decorator1, typeof(NonGenericServiceDecorator));

            Assert.IsTrue(object.ReferenceEquals(decorator1, decorator2),
                "Since the decorator is registered as singleton, the enumerable should always return the " +
                "same instance.");
        }

        [TestMethod]
        public void GetAllInstances_TypeRegisteredWithRegisterSingleFuncDecorator_AlwaysReturnsTheSameInstance()
        {
            // Arrange
            var container = new Container();

            container.RegisterAll(typeof(ICommandHandler<RealCommand>), new[] 
            { 
                typeof(RealCommandCommandHandler),
            });

            container.RegisterSingleDecorator(typeof(ICommandHandler<>), typeof(AsyncCommandHandlerProxy<>));

            // Act
            IEnumerable<ICommandHandler<RealCommand>> handlers =
                container.GetAllInstances<ICommandHandler<RealCommand>>();

            var decorator1 = handlers.Single();
            var decorator2 = handlers.Single();

            // Assert
            Assert.IsInstanceOfType(decorator1, typeof(AsyncCommandHandlerProxy<RealCommand>));

            Assert.IsTrue(object.ReferenceEquals(decorator1, decorator2),
                "Since the decorator is registered as singleton, the enumerable should always return the " +
                "same instance.");
        }

        [TestMethod]
        public void GetAllInstances_SingleFuncDecoratorDecoratedWithTransientDecorator_WorksAsExpected()
        {
            // Arrange
            var container = new Container();

            container.RegisterAll(typeof(ICommandHandler<RealCommand>), new[] 
            { 
                typeof(RealCommandCommandHandler),
            });

            container.RegisterSingleDecorator(typeof(ICommandHandler<>), typeof(AsyncCommandHandlerProxy<>));
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(TransactionalCommandHandlerDecorator<>));

            // Act
            var handlers = container.GetAllInstances<ICommandHandler<RealCommand>>();

            var stubDecorator1 = (TransactionalCommandHandlerDecorator<RealCommand>)handlers.Single();
            var stubDecorator2 = (TransactionalCommandHandlerDecorator<RealCommand>)handlers.Single();

            var asyncDecorator1 = (AsyncCommandHandlerProxy<RealCommand>)stubDecorator1.Decorated;
            var asyncDecorator2 = (AsyncCommandHandlerProxy<RealCommand>)stubDecorator1.Decorated;

            // Assert
            Assert.IsFalse(object.ReferenceEquals(stubDecorator1, stubDecorator2),
                "StubDecorator1 is registered as transient.");

            Assert.IsTrue(object.ReferenceEquals(asyncDecorator1, asyncDecorator2),
                "AsyncCommandHandlerProxy is registered as singleton.");
        }

        [TestMethod]
        public void GetAllInstances_RegisteredCollectionContainerBothTransientAsSingletons_ResolvesTransientsCorrectly()
        {
            // Arrange
            var container = new Container();

            // Register the NullCommandHandler<RealCommand> as singleton.
            // We do this using the RegisterSingleOpenGeneric, but this is not important for this test.
            container.RegisterSingleOpenGeneric(typeof(ICommandHandler<>), typeof(NullCommandHandler<>));

            // Collection that returns both a transient (RealCommandCommandHandler) and singleton.
            container.RegisterAll<ICommandHandler<RealCommand>>(
                typeof(RealCommandCommandHandler),
                typeof(NullCommandHandler<RealCommand>));

            container.RegisterDecorator(
                typeof(ICommandHandler<>),
                typeof(RealCommandCommandHandlerDecorator));

            // Act
            var handlers = container.GetAllInstances<ICommandHandler<RealCommand>>();

            var realHandler1 = ((RealCommandCommandHandlerDecorator)handlers.First()).Decorated;
            var realHandler2 = ((RealCommandCommandHandlerDecorator)handlers.First()).Decorated;

            // Assert
            Assert.IsInstanceOfType(realHandler1, typeof(RealCommandCommandHandler));

            bool isTransient = !object.ReferenceEquals(realHandler1, realHandler2);

            Assert.IsTrue(isTransient,
                "The RealCommandCommandHandler is registered as transient and should be injected as transient.");
        }

        [TestMethod]
        public void GetAllInstances_RegisteredCollectionContainerBothTransientAsSingletons_ResolvesSingletonsCorrectly()
        {
            // Arrange
            var container = new Container();

            // Register the NullCommandHandler<RealCommand> as singleton.
            container.RegisterSingle<NullCommandHandler<RealCommand>>();

            // Collection that returns both a transient (RealCommandCommandHandler) and singleton.
            container.RegisterAll<ICommandHandler<RealCommand>>(
                typeof(RealCommandCommandHandler),
                typeof(NullCommandHandler<RealCommand>));

            container.RegisterDecorator(
                typeof(ICommandHandler<>),
                typeof(RealCommandCommandHandlerDecorator));

            // Act
            IEnumerable<ICommandHandler<RealCommand>> handlers =
                container.GetAllInstances<ICommandHandler<RealCommand>>();

            var nullHandler1 = ((RealCommandCommandHandlerDecorator)handlers.Last()).Decorated;
            var nullHandler2 = ((RealCommandCommandHandlerDecorator)handlers.Last()).Decorated;

            // Assert
            Assert.IsInstanceOfType(nullHandler1, typeof(NullCommandHandler<RealCommand>));

            bool isSingleton = object.ReferenceEquals(nullHandler1, nullHandler2);

            Assert.IsTrue(isSingleton,
                "The NullCommandHandler is registered as singleton and should be injected as singleton.");
        }

        [TestMethod]
        public void GetAllInstances_CollectionDecoratedWithSingletonDecorator1_WillNotReturnAMutableType()
        {
            // Arrange
            var container = new Container();

            // Use the RegisterAll<T>(Type[]) overload.
            container.RegisterAll<ICommandHandler<RealCommand>>(typeof(RealCommandCommandHandler));

            container.RegisterSingleDecorator(
                typeof(ICommandHandler<>),
                typeof(TransactionalCommandHandlerDecorator<>));

            // Act
            var handlers = container.GetAllInstances<ICommandHandler<RealCommand>>();

            // Assert
            // Especially when registering singleton decorators, it is important that the returned collection
            // is not mutable, since changes to this collection could effect the whole application.
            Assert_IsNotAMutableCollection(handlers);
        }

        [TestMethod]
        public void GetAllInstances_CollectionDecoratedWithSingletonDecorator2_WillNotReturnAMutableType()
        {
            // Arrange
            var container = new Container();

            // Use the RegisterAll<T>(T[]) overload.
            container.RegisterAll<ICommandHandler<RealCommand>>(new[] { new RealCommandCommandHandler() });

            container.RegisterSingleDecorator(
                typeof(ICommandHandler<>),
                typeof(TransactionalCommandHandlerDecorator<>));

            // Act
            var handlers = container.GetAllInstances<ICommandHandler<RealCommand>>();

            // Assert
            Assert_IsNotAMutableCollection(handlers);
        }

        [TestMethod]
        public void GetAllInstances_CollectionDecoratedWithSingletonDecorator3_WillNotReturnAMutableType()
        {
            // Arrange
            var container = new Container();

            IEnumerable<ICommandHandler<RealCommand>> handlers = new[] { new RealCommandCommandHandler() };

            // Use the RegisterAll<T>(IEnumerable<T>) overload.
            container.RegisterAll<ICommandHandler<RealCommand>>(handlers);

            container.RegisterSingleDecorator(
                typeof(ICommandHandler<>),
                typeof(TransactionalCommandHandlerDecorator<>));

            // Act
            var actualHandlers = container.GetAllInstances<ICommandHandler<RealCommand>>();

            // Assert
            Assert_IsNotAMutableCollection(actualHandlers);
        }

        [TestMethod]
        public void GetAllInstances_CollectionDecoratedWithSingletonDecorator4_WillNotReturnAMutableType()
        {
            // Arrange
            var container = new Container();

            // Use the Register<T>(Func<T>) overload.
            container.Register<IEnumerable<ICommandHandler<RealCommand>>>(
                () => new[] { new RealCommandCommandHandler() });

            container.RegisterSingleDecorator(
                typeof(ICommandHandler<>),
                typeof(TransactionalCommandHandlerDecorator<>));

            // Act
            var handlers = container.GetAllInstances<ICommandHandler<RealCommand>>();

            // Assert
            Assert_IsNotAMutableCollection(handlers);
        }

        [TestMethod]
        public void GetAllInstances_DecoratorRegisteredWithPredicate_DecoratesInstancesThatShouldBeDecorated()
        {
            // Arrange
            var container = new Container();

            container.RegisterAll<ICommandHandler<RealCommand>>(
                typeof(RealCommandCommandHandler),
                typeof(NullCommandHandler<RealCommand>));

            container.RegisterDecorator(
                typeof(ICommandHandler<>),
                typeof(TransactionalCommandHandlerDecorator<>),
                c => c.ImplementationType == typeof(RealCommandCommandHandler));

            // Act
            var handlers = container.GetAllInstances<ICommandHandler<RealCommand>>();

            var handler = handlers.First();

            // Assert
            bool realCommandHandlerIsDecorated =
                handler.GetType() == typeof(TransactionalCommandHandlerDecorator<RealCommand>);

            Assert.IsTrue(realCommandHandlerIsDecorated);
        }

        [TestMethod]
        public void GetAllInstances_DecoratorRegisteredWithPredicate_DoesNotDecoratesInstancesThatShouldNotBeDecorated()
        {
            // Arrange
            var container = new Container();

            container.RegisterAll<ICommandHandler<RealCommand>>(
                typeof(RealCommandCommandHandler),
                typeof(NullCommandHandler<RealCommand>));

            container.RegisterDecorator(
                typeof(ICommandHandler<>),
                typeof(TransactionalCommandHandlerDecorator<>),
                c => c.ImplementationType == typeof(RealCommandCommandHandler));

            // Act
            var handlers = container.GetAllInstances<ICommandHandler<RealCommand>>();

            var handler = handlers.Last();

            // Assert
            Assert.IsInstanceOfType(handler, typeof(NullCommandHandler<RealCommand>));

            bool nullCommandHandlerIsDecorated =
                handler.GetType() == typeof(TransactionalCommandHandlerDecorator<RealCommand>);

            Assert.IsFalse(nullCommandHandlerIsDecorated);
        }

        [TestMethod]
        public void GetAllInstances_DecoratorRegisteredWithPredicate_DecoratesAllInstancesThatShouldBeDecorated()
        {
            // Arrange
            var container = new Container();

            container.RegisterAll<ICommandHandler<RealCommand>>(
                typeof(RealCommandCommandHandler),
                typeof(DefaultCommandHandler<RealCommand>),
                typeof(NullCommandHandler<RealCommand>));

            container.RegisterDecorator(
                typeof(ICommandHandler<>),
                typeof(TransactionalCommandHandlerDecorator<>), context =>
                {
                    var name = context.ImplementationType.Name;

                    return name.StartsWith("Default") || name.StartsWith("Null");
                });

            // Act
            var handlers = container.GetAllInstances<ICommandHandler<RealCommand>>().ToArray();

            Assert.AreEqual(3, handlers.Length, "Not the correct number of handlers was returned.");

            var realHandler = handlers[0];
            var defaultHandler = handlers[1];
            var nullHandler = handlers[2];

            // Assert
            Assert.IsInstanceOfType(realHandler, typeof(RealCommandCommandHandler),
                "The RealCommandCommandHandler was expected not to be decorated.");
            Assert.IsInstanceOfType(defaultHandler, typeof(TransactionalCommandHandlerDecorator<RealCommand>),
                "The DefaultCommandHandler was expected to be decorated.");
            Assert.IsInstanceOfType(nullHandler, typeof(TransactionalCommandHandlerDecorator<RealCommand>),
                "The NullCommandHandler was expected to be decorated.");
        }

        [TestMethod]
        public void GetAllInstances_MultipleDecoratorsRegisteredWithPredicate_DecoratesInstancesThatShouldBeDecorated()
        {
            // Arrange
            var container = new Container();

            container.RegisterAll<ICommandHandler<RealCommand>>(
                typeof(RealCommandCommandHandler),
                typeof(NullCommandHandler<RealCommand>));

            container.RegisterDecorator(
                typeof(ICommandHandler<>),
                typeof(TransactionalCommandHandlerDecorator<>),
                c => c.ImplementationType == typeof(RealCommandCommandHandler));

            container.RegisterSingleDecorator(
                typeof(ICommandHandler<>),
                typeof(AsyncCommandHandlerProxy<>),
                c => c.ImplementationType == typeof(RealCommandCommandHandler));

            // Act
            var handlers = container.GetAllInstances<ICommandHandler<RealCommand>>();

            var handler = handlers.First();

            // Assert
            bool realCommandHandlerIsDecorated =
                handler.GetType() == typeof(AsyncCommandHandlerProxy<RealCommand>);

            Assert.IsTrue(realCommandHandlerIsDecorated);
        }

        [TestMethod]
        public void GetAllInstances_MultipleDecoratorsRegisteredWithPredicate2_DecoratesInstancesThatShouldBeDecorated()
        {
            // Arrange
            var container = new Container();

            container.RegisterAll<ICommandHandler<RealCommand>>(
                typeof(RealCommandCommandHandler),
                typeof(NullCommandHandler<RealCommand>));

            container.RegisterDecorator(
                typeof(ICommandHandler<>),
                typeof(TransactionalCommandHandlerDecorator<>),
                c => c.ImplementationType == typeof(RealCommandCommandHandler));

            container.RegisterSingleDecorator(
                typeof(ICommandHandler<>),
                typeof(AsyncCommandHandlerProxy<>),
                c => c.ImplementationType == typeof(NullCommandHandler<RealCommand>));

            // Act
            var realHandler1 = container.GetAllInstances<ICommandHandler<RealCommand>>().First();
            var realHandler2 = container.GetAllInstances<ICommandHandler<RealCommand>>().First();

            var nullHandler1 = container.GetAllInstances<ICommandHandler<RealCommand>>().Last();
            var nullHandler2 = container.GetAllInstances<ICommandHandler<RealCommand>>().Last();

            // Assert
            Assert.IsInstanceOfType(realHandler1, typeof(TransactionalCommandHandlerDecorator<RealCommand>),
                "RealCommandCommandHandler hasn't been decorated properly.");

            Assert.IsInstanceOfType(nullHandler1, typeof(AsyncCommandHandlerProxy<RealCommand>),
                "NullCommandHandler hasn't been decorated properly.");

            Assert.IsFalse(object.ReferenceEquals(realHandler1, realHandler2),
                "TransactionalCommandHandlerDecorator is registered as transient and should therefore be transient.");
            Assert.IsTrue(object.ReferenceEquals(nullHandler1, nullHandler2),
                "AsyncCommandHandlerProxy is registered as singleton and should therefore be singleton.");
        }

        [TestMethod]
        public void GetAllInstances_PredicateDecorator_PredicateGetsSuppliedWithExpectedAppliedDecoratorsCollection()
        {
            // Arrange
            int predicateCallCount = 0;

            var container = new Container();

            container.RegisterAll<ICommandHandler<RealCommand>>(
                typeof(RealCommandCommandHandler),
                typeof(NullCommandHandler<RealCommand>));

            container.RegisterDecorator(
                typeof(ICommandHandler<>),
                typeof(TransactionalCommandHandlerDecorator<>));

            container.RegisterSingleDecorator(
                typeof(ICommandHandler<>),
                typeof(AsyncCommandHandlerProxy<>), context =>
            {
                predicateCallCount++;

                // Assert
                Assert.AreEqual(typeof(TransactionalCommandHandlerDecorator<RealCommand>),
                    context.AppliedDecorators.Single());

                return true;
            });

            // Act
            container.GetAllInstances<ICommandHandler<RealCommand>>().ToArray();

            // Assert
            Assert.AreEqual(2, predicateCallCount, "The predicate is expected to be called once per handler.");
        }

        [TestMethod]
        public void GetAllInstances_PredicateDecorator_AppliedDecoratorsIsEmpyWhenNoDecoratorsHaveBeenAppliedYet()
        {
            // Arrange
            int predicateCallCount = 0;

            var container = new Container();

            container.RegisterAll<ICommandHandler<RealCommand>>(
                typeof(RealCommandCommandHandler),
                typeof(NullCommandHandler<RealCommand>));

            container.RegisterDecorator(
                typeof(ICommandHandler<>),
                typeof(TransactionalCommandHandlerDecorator<>),
                context => context.ImplementationType == typeof(RealCommandCommandHandler));

            container.RegisterSingleDecorator(
                typeof(ICommandHandler<>),
                typeof(LifetimeScopeCommandHandlerProxy<>),
                context => context.ImplementationType == typeof(RealCommandCommandHandler));

            container.RegisterSingleDecorator(
                typeof(ICommandHandler<>),
                typeof(AsyncCommandHandlerProxy<>), context =>
            {
                if (context.ImplementationType == typeof(NullCommandHandler<RealCommand>))
                {
                    predicateCallCount++;

                    Assert.AreEqual(0, context.AppliedDecorators.Count(),
                        "No decorators have been applied to the NullCommandHandler and the " +
                        "AppliedDecorators collection is expected to be empty.");
                }

                return true;
            });

            // Act
            container.GetAllInstances<ICommandHandler<RealCommand>>().ToArray();

            // Assert
            Assert.AreEqual(1, predicateCallCount, "The predicate is expected to be called just once.");
        }

        [TestMethod]
        public void GetAllInstances_InstancesRegisteredWithRegisterAllParamsTAndDecorated_InjectsSingletons()
        {
            // Arrange
            var container = new Container();

            // Uses the RegisterAll<T>(params T[]) that explicitly registers a collection of singletons.
            container.RegisterAll<ICommandHandler<RealCommand>>(
                new RealCommandCommandHandler(),
                new NullCommandHandler<RealCommand>());

            container.RegisterDecorator(
                typeof(ICommandHandler<>),
                typeof(TransactionalCommandHandlerDecorator<>));

            // Act
            var decorator = container.GetAllInstances<ICommandHandler<RealCommand>>().First();

            // Assert
            Assert.IsInstanceOfType(decorator, typeof(TransactionalCommandHandlerDecorator<RealCommand>));
        }

        [TestMethod]
        public void GetAllInstances_InstancesRegisteredWithRegisterAllParamsTAndDecorated_SuppliesTheCorrectPredicateContextForEachElement()
        {
            // Arrange
            var container = new Container();

            var predicateContexts = new List<DecoratorPredicateContext>();

            // Uses the RegisterAll<T>(params T[]) that explicitly registers a collection of singletons.
            container.RegisterAll<ICommandHandler<RealCommand>>(
                new RealCommandCommandHandler(),
                new NullCommandHandler<RealCommand>());

            container.RegisterDecorator(
                typeof(ICommandHandler<>),
                typeof(TransactionalCommandHandlerDecorator<>), context =>
                {
                    predicateContexts.Add(context);

                    return false;
                });

            // Act
            container.GetAllInstances<ICommandHandler<RealCommand>>().ToArray();
            container.GetAllInstances<ICommandHandler<RealCommand>>().ToArray();
            container.GetAllInstances<ICommandHandler<RealCommand>>().ToArray();
            container.GetAllInstances<ICommandHandler<RealCommand>>().ToArray();
            container.GetAllInstances<ICommandHandler<RealCommand>>().ToArray();

            // Assert
            Assert.AreEqual(2, predicateContexts.Count, "Since the registration is made with an array of " +
                "singletons, the decorator system should have enough information to call the predicate " +
                "once for each element in the collection.");

            DecoratorPredicateContext realContext = predicateContexts[0];

            Assert.AreEqual(realContext.ServiceType, typeof(ICommandHandler<RealCommand>));
            Assert.AreEqual(realContext.ImplementationType, typeof(RealCommandCommandHandler));
            Assert.IsInstanceOfType(realContext.Expression, typeof(ConstantExpression));
            Assert.AreEqual(0, realContext.AppliedDecorators.Count);

            DecoratorPredicateContext nullContext = predicateContexts[1];

            Assert.AreEqual(nullContext.ServiceType, typeof(ICommandHandler<RealCommand>));
            Assert.AreEqual(nullContext.ImplementationType, typeof(NullCommandHandler<RealCommand>));
            Assert.IsInstanceOfType(nullContext.Expression, typeof(ConstantExpression));
            Assert.AreEqual(0, nullContext.AppliedDecorators.Count);
        }

        [TestMethod]
        public void GetAllInstances_InstancesRegisteredWithRegisterAllEnumerableAndDecorated_CallsThePredicateJustOnceForTheWholeCollection()
        {
            // Arrange
            var container = new Container();

            var predicateContexts = new List<DecoratorPredicateContext>();

            IEnumerable<ICommandHandler<RealCommand>> dynamicList = new List<ICommandHandler<RealCommand>>
            {
                new RealCommandCommandHandler(),
                new NullCommandHandler<RealCommand>()
            };

            // Uses the RegisterAll<T>(IEnumerable<T>) that registers a dynamic list.
            container.RegisterAll<ICommandHandler<RealCommand>>(dynamicList);

            container.RegisterDecorator(
                typeof(ICommandHandler<>),
                typeof(TransactionalCommandHandlerDecorator<>), context =>
                {
                    predicateContexts.Add(context);

                    return false;
                });

            // Act
            container.GetAllInstances<ICommandHandler<RealCommand>>().ToArray();
            container.GetAllInstances<ICommandHandler<RealCommand>>().ToArray();
            container.GetAllInstances<ICommandHandler<RealCommand>>().ToArray();
            container.GetAllInstances<ICommandHandler<RealCommand>>().ToArray();
            container.GetAllInstances<ICommandHandler<RealCommand>>().ToArray();

            // Assert
            Assert.AreEqual(1, predicateContexts.Count, "The predicate should therefore be called " +
                "just once when collections are registered using RegisterAll(IEnumerable).");

            DecoratorPredicateContext collectionContext = predicateContexts.Single();

            AssertThat.AreEqual(typeof(ICommandHandler<RealCommand>), collectionContext.ServiceType);
            AssertThat.AreEqual(typeof(ICommandHandler<RealCommand>), collectionContext.ImplementationType,
                "Since there is no information about the elements of the collection (and they can change) " +
                "there is no information about the implementation type, and the service type should be applied.");

            Assert.AreEqual(0, collectionContext.AppliedDecorators.Count);
        }

        [TestMethod]
        public void GetAllInstances_InstancesRegisteredWithRegisterAllEnumerableAndDecoratedWithMultipleDecorators_SuppliesThePreviouslyAppliedDecoratorsToThePredicate()
        {
            // Arrange
            var container = new Container();

            int predicateCallCount = 0;

            IEnumerable<ICommandHandler<RealCommand>> dynamicList = new List<ICommandHandler<RealCommand>>
            {
                new RealCommandCommandHandler(),
                new NullCommandHandler<RealCommand>()
            };

            // Uses the RegisterAll<T>(IEnumerable<T>) that registers a dynamic list.
            container.RegisterAll<ICommandHandler<RealCommand>>(dynamicList);

            container.RegisterDecorator(
                typeof(ICommandHandler<>),
                typeof(TransactionalCommandHandlerDecorator<>));

            container.RegisterDecorator(
                typeof(ICommandHandler<>),
                typeof(RealCommandCommandHandlerDecorator), context =>
            {
                predicateCallCount++;

                // Assert
                Assert.AreEqual(1, context.AppliedDecorators.Count);
                Assert.AreEqual(context.AppliedDecorators.Single(),
                    typeof(TransactionalCommandHandlerDecorator<RealCommand>));

                return false;
            });

            // Act
            container.GetAllInstances<ICommandHandler<RealCommand>>().ToArray();
            container.GetAllInstances<ICommandHandler<RealCommand>>().ToArray();
            container.GetAllInstances<ICommandHandler<RealCommand>>().ToArray();
            container.GetAllInstances<ICommandHandler<RealCommand>>().ToArray();
            container.GetAllInstances<ICommandHandler<RealCommand>>().ToArray();

            // Assert
            Assert.AreEqual(1, predicateCallCount);
        }

        [TestMethod]
        public void GetAllInstances_InstancesRegisteredWithRegisterAllEnumerableAndDecoratedWithMultipleDecorators_DoesNotSupplyThePreviousDecoratorWhenItWasNotApplied()
        {
            // Arrange
            var container = new Container();

            int predicateCallCount = 0;

            IEnumerable<ICommandHandler<RealCommand>> dynamicList = new List<ICommandHandler<RealCommand>>
            {
                new RealCommandCommandHandler(),
                new NullCommandHandler<RealCommand>()
            };

            // Uses the RegisterAll<T>(IEnumerable<T>) that registers a dynamic list.
            container.RegisterAll<ICommandHandler<RealCommand>>(dynamicList);

            container.RegisterDecorator(
                typeof(ICommandHandler<>),
                typeof(TransactionalCommandHandlerDecorator<>), c => false);

            container.RegisterDecorator(
                typeof(ICommandHandler<>),
                typeof(RealCommandCommandHandlerDecorator), context =>
                {
                    predicateCallCount++;

                    // Assert
                    Assert.AreEqual(0, context.AppliedDecorators.Count,
                        "No decorators expected since, the previous decorator is not applied.");

                    return false;
                });

            // Act
            container.GetAllInstances<ICommandHandler<RealCommand>>().ToArray();

            // Assert
            Assert.AreEqual(1, predicateCallCount);
        }

        [TestMethod]
        public void GetInstance_BothServiceAndCollectionOfServicesRegistered_RegistrationsDontShareTheirPredicateContext()
        {
            // Arrange
            var container = new Container();

            int predicateCallCount = 0;

            IEnumerable<ICommandHandler<RealCommand>> dynamicList = new List<ICommandHandler<RealCommand>>
            {
                new RealCommandCommandHandler(),
                new NullCommandHandler<RealCommand>()
            };

            container.Register<ICommandHandler<RealCommand>, RealCommandCommandHandler>();

            // Uses the RegisterAll<T>(IEnumerable<T>) that registers a dynamic list.
            container.RegisterAll<ICommandHandler<RealCommand>>(dynamicList);

            container.RegisterDecorator(
                typeof(ICommandHandler<>),
                typeof(TransactionalCommandHandlerDecorator<>));

            container.RegisterDecorator(
                typeof(ICommandHandler<>),
                typeof(RealCommandCommandHandlerDecorator), context =>
                {
                    predicateCallCount++;

                    // Assert
                    Assert.AreEqual(1, context.AppliedDecorators.Count,
                        "One decorator was expected to be applied. Applied decorators: " +
                        string.Join(", ", context.AppliedDecorators.Select(d => d.Name).ToArray()));

                    Assert.AreEqual(context.AppliedDecorators.Single(),
                        typeof(TransactionalCommandHandlerDecorator<RealCommand>));

                    return false;
                });

            // Act
            container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            Assert.AreEqual(1, predicateCallCount);

            // Act
            container.GetAllInstances<ICommandHandler<RealCommand>>();

            // Assert
            Assert.AreEqual(2, predicateCallCount);
        }

        [TestMethod]
        public void GetAllInstances_DecoratorRegisteredTwiceAsSingleton_WrapsTheDecorateeTwice()
        {
            // Arrange
            var container = new Container();

            IEnumerable<ICommandHandler<RealCommand>> dynamicList = new List<ICommandHandler<RealCommand>>
            {
                new RealCommandCommandHandler(),
                new NullCommandHandler<RealCommand>()
            };

            // Uses the RegisterAll<T>(IEnumerable<T>) that registers a dynamic list.
            container.RegisterAll<ICommandHandler<RealCommand>>(dynamicList);

            // Register the same decorator twice. 
            container.RegisterSingleDecorator(
                typeof(ICommandHandler<>),
                typeof(TransactionalCommandHandlerDecorator<>));

            container.RegisterSingleDecorator(
                typeof(ICommandHandler<>),
                typeof(TransactionalCommandHandlerDecorator<>));

            // Act
            var decorator1 = (TransactionalCommandHandlerDecorator<RealCommand>)
                container.GetAllInstances<ICommandHandler<RealCommand>>().First();

            var decorator2 = decorator1.Decorated;

            // Assert
            Assert.IsInstanceOfType(decorator2, typeof(TransactionalCommandHandlerDecorator<RealCommand>),
                "Since the decorator is registered twice, it should wrap the decoratee twice.");

            var decoratee = ((TransactionalCommandHandlerDecorator<RealCommand>)decorator2).Decorated;

            Assert.IsInstanceOfType(decoratee, typeof(RealCommandCommandHandler));
        }

        private static void
            Assert_ExceptionContainsInfoAboutManualCollectionRegistrationMixedDecoratorsThatTakeAFunc(
            ActivationException ex)
        {
            AssertThat.StringContains(
                "impossible for the container to generate a Func<ICommandHandler<RealCommand>> for " +
                "injection into the AsyncCommandHandlerProxy<T> decorator",
                ex.Message);

            AssertThat.StringContains(
                "the registration hasn't been made using one of the RegisterAll overloads that take " +
                "a list of System.Type",
                ex.Message);

            AssertThat.StringContains(
                "switch to one of the other RegisterAll overloads, or don't use a decorator that " +
                "depends on a Func<T>",
                ex.Message);
        }

        private static void Assert_IsNotAMutableCollection(object collection)
        {
            Type type = collection.GetType();

            Type genericTypeDefinition = type.IsGenericType ? type.GetGenericTypeDefinition() : null;

            if (genericTypeDefinition == typeof(ReadOnlyCollection<>))
            {
                return;
            }

            if (genericTypeDefinition == typeof(ICollection<>) || type == typeof(IList) || type.IsArray)
            {
                Assert.Fail("The {0} is a mutable type", type.Name);
            }
        }

        public class RealNonGenericService : INonGenericService
        {
            public void DoSomething()
            {
            }
        }

        public class NonGenericServiceDecorator : INonGenericService
        {
            public NonGenericServiceDecorator(INonGenericService decorated)
            {
                this.DecoratedService = decorated;
            }

            public INonGenericService DecoratedService { get; private set; }

            public void DoSomething()
            {
                this.DecoratedService.DoSomething();
            }
        }

        public class RealCommand
        {
        }

        public class NullCommandHandler<T> : ICommandHandler<T>
        {
            public void Handle(T command)
            {
            }
        }

        public class DefaultCommandHandler<T> : ICommandHandler<T>
        {
            public void Handle(T command)
            {
            }
        }

        public class RealCommandCommandHandler : ICommandHandler<RealCommand>
        {
            public void Handle(RealCommand command)
            {
            }
        }

        public class AsyncCommandHandlerProxy<T> : ICommandHandler<T>
        {
            public AsyncCommandHandlerProxy(Container container, Func<ICommandHandler<T>> decorateeFactory)
            {
                this.DecorateeFactory = decorateeFactory;
            }

            public Func<ICommandHandler<T>> DecorateeFactory { get; private set; }

            public void Handle(T command)
            {
                // Run decorated instance on new thread (not important for these tests).
            }
        }

        public class LifetimeScopeCommandHandlerProxy<T> : ICommandHandler<T>
        {
            public LifetimeScopeCommandHandlerProxy(Func<ICommandHandler<T>> decorateeFactory,
                Container container)
            {
                this.DecorateeFactory = decorateeFactory;
            }

            public Func<ICommandHandler<T>> DecorateeFactory { get; private set; }

            public void Handle(T command)
            {
                // Start lifetime scope here (not important for these tests).
            }
        }

        public class TransactionalCommandHandlerDecorator<T> : ICommandHandler<T>
        {
            public TransactionalCommandHandlerDecorator(ICommandHandler<T> decorated)
            {
                this.Decorated = decorated;
            }

            public ICommandHandler<T> Decorated { get; private set; }

            public void Handle(T command)
            {
                // Start a transaction (not important for these tests).
            }
        }

        public class RealCommandCommandHandlerDecorator : ICommandHandler<RealCommand>
        {
            public RealCommandCommandHandlerDecorator(ICommandHandler<RealCommand> decoratedHandler)
            {
                this.Decorated = decoratedHandler;
            }

            public ICommandHandler<RealCommand> Decorated { get; private set; }

            public void Handle(RealCommand command)
            {
            }
        }
    }
}