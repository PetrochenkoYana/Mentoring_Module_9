using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TestHelper;
using StaticCode_Analize;
using System.Web.Mvc;
using System.Runtime.Serialization;

namespace StaticCode_Analize.Test
{
    [TestClass]
    public class UnitTest : CodeFixVerifier
    {

        //No diagnostics expected to show up
        [TestMethod]
        public void TestMethod1()
        {
            var test = @"";

            VerifyCSharpDiagnostic(test);
        }

        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public void TestMethod2()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TypeName
        {   
        }
    }";
            var expected = new DiagnosticResult
            {
                Id = "StaticCode_Analize",
                Message = String.Format("Type name '{0}' contains lowercase letters", "TypeName"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 11, 15)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TYPENAME
        {   
        }
    }";
            VerifyCSharpFix(test, fixtest);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new StaticCode_AnalizeCodeFixProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new StaticCode_AnalizeAnalyzer();
        }

        [TestMethod]
        public void CheckControllersNames()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using System.Web.Mvc;

    namespace ConsoleApplication1
    {
        class BaseController : System.Web.Mvc.Controller
        {   
        }

        class BaseControlle : Controller
        {   
        }
    }";
            var expected = new DiagnosticResult
            {
                Id = "StaticCode_Analize",
                Message = String.Format("Type name '{0}' does not end with 'Controller'", "BaseControlle"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 16, 15)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void CheckControllersAuthorizedAttribute()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using System.Web.Mvc;

    namespace ConsoleApplication1
    {
        [Authorize] 
        class BaseController : System.Web.Mvc.Controller
        {   
        }

        class BaseControlle : Controller
        {   
        }

        class BaseControll : Controller
        {   [Authorize]
            public void AuthorizedMethod(){}
        }
        class BaseCont : Controller
        {   
            public void AuthorizedMethod(){}
        }
    }";
            DiagnosticResult[] expected = new DiagnosticResult[2];
            expected[0] = new DiagnosticResult
            {
                Id = "StaticCode_Analize",
                Message = String.Format("Type name '{0}' does marked by 'Authorize' attribute as well as all its public methods", "BaseControlle"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 17, 15)
                        }
            };
            expected[1] = new DiagnosticResult
            {
                Id = "StaticCode_Analize",
                Message = String.Format("Type name '{0}' does marked by 'Authorize' attribute as well as all its public methods", "BaseCont"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 25, 15)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);
        }


        [TestMethod]
        public void EntitiesNamespaceChecks()
        {
                    var test = @"
            using System;
            using System.Collections.Generic;
            using System.Linq;
            using System.Text;
            using System.Threading.Tasks;
            using System.Diagnostics;
            using System.Web.Mvc;
            using System.Runtime.Serialization;

            namespace ConsoleApplication1.Entities
            {
                  [DataContract]
                    public class BaseController : System.Web.Mvc.Controller
                    {
                        public int Id { get; set; }
                        public string Name { get; set; }
                    }

                    [DataContract]
                    class BaseControlle : Controller
                    {
                        public int Id { get; set; }
                        public string Name { get; set; }
                    }

                    public class BaseControll : Controller
                    {
                        public int Id { get; set; }
                        public string Name { get; set; }
                    }

                    [DataContract]
                    public class BaseCont : Controller
                    {
                    }
       
    }";
            DiagnosticResult[] expected = new DiagnosticResult[3];
            expected[0] = new DiagnosticResult
            {
                Id = "StaticCode_Analize",
                Message = String.Format("Type name '{0}' does not meet rules.", "BaseControlle"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 21, 27)
                        }
            };
            expected[1] = new DiagnosticResult
            {
                Id = "StaticCode_Analize",
                Message = String.Format("Type name '{0}' does not meet rules.", "BaseControll"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 27, 34)
                        }
            };
            expected[2] = new DiagnosticResult
            {
                Id = "StaticCode_Analize",
                Message = String.Format("Type name '{0}' does not meet rules.", "BaseCont"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                   new[] {
                            new DiagnosticResultLocation("Test0.cs", 34, 34)
                       }
            };

            VerifyCSharpDiagnostic(test, expected);
        }
    }
}
