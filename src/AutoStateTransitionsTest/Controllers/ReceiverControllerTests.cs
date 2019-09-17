using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

using Moq;
using AutoStateTransitions.Repos;
using AutoStateTransitions.Misc;
using AutoStateTransitions.Repos.Interfaces;

namespace AutoStateTransitionsTest.Controllers
{
    [TestClass]
    public class ReceiverControllerTests
    {
        private Mock<IWorkItemRepo> _mockWorkItemRepo;
        private Mock<IRulesRepo> _mockRulesRepo;
        private Mock<IHelper> _mockHelper;
    }
}
