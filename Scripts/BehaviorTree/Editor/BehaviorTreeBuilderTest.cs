﻿using Adnc.FluidBT.TaskParents.Composites;
using Adnc.FluidBT.Tasks;
using Adnc.FluidBT.Tasks.Actions;
using NUnit.Framework;

namespace Adnc.FluidBT.Trees.Testing {
    public class BehaviorTreeBuilderTest {
        private int _invokeCount;
        private BehaviorTreeBuilder _builder;
            
        [SetUp]
        public void BeforeEach () {
            _invokeCount = 0;
            _builder = new BehaviorTreeBuilder(null);
        }
        
        public class SequenceMethod : BehaviorTreeBuilderTest {
            [Test]
            public void Create_a_sequence () {
                var tree = _builder
                    .Sequence()
                        .Do("action", () => {
                            _invokeCount++;
                            return TaskStatus.Success;
                        })
                    .Build();
    
                var sequence = tree.Root.Children[0] as Sequence;
                
                Assert.AreEqual(tree.Root.Children.Count, 1);
                Assert.AreEqual(sequence.Children.Count, 1);
                Assert.AreEqual(TaskStatus.Success, tree.Tick());
                Assert.AreEqual(1, _invokeCount);
            }
            
            [Test]
            public void Create_a_nested_sequence () {
                var tree = _builder
                    .Sequence("sequence")
                        .Do("action", () => {
                            _invokeCount++;
                            return TaskStatus.Success;
                        })
                        .Sequence("nested")
                            .Do("actionNested", () => {
                                _invokeCount++;
                                return TaskStatus.Success;
                            })
                    .Build();
    
                var sequence = tree.Root.Children[0] as Sequence;
                Assert.AreEqual(2, sequence.Children.Count);
                
                var nested = sequence.Children[1] as Sequence;
                Assert.AreEqual(nested.Children.Count, 1);
                Assert.AreEqual(TaskStatus.Success, tree.Tick());
                Assert.AreEqual(2, _invokeCount);
            }
            
            [Test]
            public void Create_a_nested_sequence_then_add_an_action_to_the_parent () {
                var tree = _builder
                    .Sequence("sequence")
                        .Do("action", () => {
                            _invokeCount++;
                            return TaskStatus.Success;
                        })
                        .Sequence("nested")
                            .Do("actionNested", () => {
                                _invokeCount++;
                                return TaskStatus.Success;
                            })
                        .End()
                        .Do("lastAction", () => {
                        _invokeCount++;
                            return TaskStatus.Success;
                        })
                    .Build();
    
                var sequence = tree.Root.Children[0] as Sequence;
                Assert.AreEqual(3, sequence.Children.Count);
                
                var nested = sequence.Children[1] as Sequence;
                Assert.AreEqual(nested.Children.Count, 1);
                Assert.AreEqual(TaskStatus.Success, tree.Tick());
                Assert.AreEqual(3, _invokeCount);
            }
    
            [Test]
            public void Create_two_nested_sequences_with_actions () {
                var tree = _builder
                    .Sequence("sequence")
                        .Sequence("nested")
                            .Do("actionNested", () => {
                                _invokeCount++;
                                return TaskStatus.Success;
                            })
                        .End()
                        .Sequence("nested")
                            .Do("actionNested", () => {
                                _invokeCount++;
                                return TaskStatus.Success;
                            })
                    .Build();
    
                var sequence = tree.Root.Children[0] as Sequence;
                Assert.AreEqual(2, sequence.Children.Count);
                
                var nested = sequence.Children[0] as Sequence;
                Assert.AreEqual(nested.Children.Count, 1);
                
                var nestedAlt = sequence.Children[1] as Sequence;
                Assert.AreEqual(nestedAlt.Children.Count, 1);
                
                Assert.AreEqual(TaskStatus.Success, tree.Tick());
                Assert.AreEqual(2, _invokeCount);
            }
        }

        public class SelectorMethod : BehaviorTreeBuilderTest {
            [Test]
            public void Create_a_selector () {
                var tree = _builder
                    .Selector("selector")
                        .Do("action", () => {
                            _invokeCount++;
                            return TaskStatus.Failure;
                        })
                        .Do("action", () => {
                            _invokeCount++;
                            return TaskStatus.Success;
                        })
                    .Build();
    
                var selector = tree.Root.Children[0] as Selector;
                
                Assert.AreEqual(tree.Root.Children.Count, 1);
                Assert.AreEqual(selector.Children.Count, 2);
                Assert.AreEqual(TaskStatus.Success, tree.Tick());
                Assert.AreEqual(2, _invokeCount);
            }
        }

        public class ParallelMethod : BehaviorTreeBuilderTest {
            [Test]
            public void Create_a_selector () {
                var tree = _builder
                    .Parallel()
                        .Do("action", () => {
                            _invokeCount++;
                            return TaskStatus.Success;
                        })
                        .Do("action", () => {
                            _invokeCount++;
                            return TaskStatus.Success;
                        })
                    .Build();
    
                var parallel = tree.Root.Children[0] as Parallel;
                
                Assert.AreEqual(tree.Root.Children.Count, 1);
                Assert.AreEqual(parallel.Children.Count, 2);
                Assert.AreEqual(TaskStatus.Success, tree.Tick());
                Assert.AreEqual(2, _invokeCount);
            }
        }

        public class ConditionMethod : BehaviorTreeBuilderTest {
            [Test]
            public void It_should_add_a_condition () {
                var tree = _builder
                    .Sequence()
                        .Condition("condition", () => {
                            _invokeCount++;
                            return true;
                        })
                        .Do("action", () => {
                            _invokeCount++;
                            return TaskStatus.Success;
                        })
                    .Build();
    
                var sequence = tree.Root.Children[0] as Sequence;
                var condition = sequence.Children[0] as ConditionGeneric;

                Assert.AreEqual(sequence.Children.Count, 2);
                Assert.IsNotNull(condition);
                Assert.AreEqual(TaskStatus.Success, tree.Tick());
                Assert.AreEqual(2, _invokeCount);
            }
            
            [Test]
            public void It_should_add_a_condition_without_a_name () {
                var tree = _builder
                    .Condition(() => {
                        _invokeCount++;
                        return true;
                    })
                    .Build();
    
                var condition = tree.Root.Children[0] as ConditionGeneric;

                Assert.IsNotNull(condition);
                Assert.AreEqual(TaskStatus.Success, tree.Tick());
                Assert.AreEqual(1, _invokeCount);
            }
        }
        
        public class DoMethod : BehaviorTreeBuilderTest {
            [Test]
            public void It_should_add_an_action () {
                var tree = _builder
                    .Do("action", () => {
                        _invokeCount++;
                        return TaskStatus.Success;
                    })
                    .Build();
    
                var action = tree.Root.Children[0] as ActionGeneric;

                Assert.IsNotNull(action);
                Assert.AreEqual(TaskStatus.Success, tree.Tick());
                Assert.AreEqual(1, _invokeCount);
            }
            
            [Test]
            public void It_should_add_an_action_without_a_name () {
                var tree = _builder
                    .Do(() => {
                        _invokeCount++;
                        return TaskStatus.Success;
                    })
                    .Build();
    
                var action = tree.Root.Children[0] as ActionGeneric;

                Assert.IsNotNull(action);
                Assert.AreEqual(TaskStatus.Success, tree.Tick());
                Assert.AreEqual(1, _invokeCount);
            }
        }
    }
}