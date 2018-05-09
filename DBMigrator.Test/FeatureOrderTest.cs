﻿using DBMigrator.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DBMigrator.Test
{
    [TestClass]
    public class FeatureOrderTest
    {
        [TestMethod]
        public void feature_with_order_test()
        {
            var (order, featurename) = Feature.GetFeatureNameAndOrder("01_Test");

            Assert.AreEqual(1, order);
            Assert.AreEqual("Test", featurename);
        }

        [TestMethod]
        public void feature_without_order_test()
        {
            var (order, featurename) = Feature.GetFeatureNameAndOrder("Test2");

            Assert.AreEqual(0, order);
            Assert.AreEqual("Test2", featurename);
        }

        [TestMethod]
        public void complex_featurename_test()
        {
            var (order, featurename) = Feature.GetFeatureNameAndOrder("DF-26261_Fix_Spelling");

            Assert.AreEqual(0, order);
            Assert.AreEqual("DF-26261_Fix_Spelling", featurename);
        }

        [TestMethod]
        public void no_more_then_99_ordering_test()
        {
            var (order, featurename) = Feature.GetFeatureNameAndOrder("26261_Fix_Spelling");

            Assert.AreEqual(0, order);
            Assert.AreEqual("26261_Fix_Spelling", featurename);
        }

    }
}
