using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Windows.Forms;
using System.Drawing;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UITest.Extension;
using Keyboard = Microsoft.VisualStudio.TestTools.UITesting.Keyboard;

//Make sure to delete the AllTest and UITest files after running if you plan on doing
//another test afterwards.  The commented out methods are methods that caused errors and
//forced me to start their part over again.  Unfortunately I wasn't able to delete them
//because I could not find all the code associated with them as trying to run after deleting
//the code that I could find resulted in the entire thing exploding.

namespace UITesting
{
    /// <summary>
    /// Summary description for CodedUITest1
    /// </summary>
    [CodedUITest]
    public class CodedUITest1
    {
        public CodedUITest1()
        {
        }

        [TestMethod]
        public void CodedUITestMethod1()
        {
            // To generate code for this test, select "Generate Code for Coded UI Test" from the shortcut menu and select one of the menu items.
            ApplicationUnderTest.Launch(@"../../../SpreadsheetGUI/bin/debug/SpreadsheetGUI.exe");
            this.UIMap.BasicFunctions();
            this.UIMap.AssertBasicFunctions();
            this.UIMap.EditMenu();
            this.UIMap.AssertEditMenu();
            this.UIMap.OpenHelp();
            this.UIMap.AssertHelpOpen();
            this.UIMap.CloseHelp();
            this.UIMap.ErrorMessageAppear();
            this.UIMap.AssertErrorMessage();
            this.UIMap.CloseErrorMessage();
            this.UIMap.FileNew();
            this.UIMap.ClosingSpreadsheets();
            this.UIMap.AssertClosingSafety();
            this.UIMap.SavingFunctions();
            this.UIMap.AssertSaving();
            this.UIMap.ReplaceHandler();
            this.UIMap.OpenPrepMethod();
            this.UIMap.OpenSprdFile();
            this.UIMap.AssertSprdFileOpen();
            this.UIMap.AllFileOpen();
            this.UIMap.AssertAllFileOpened();
            this.UIMap.CloseApp();




            //this.UIMap.AssertSprdOpened();


            //this.UIMap.OpenMethod();
            //this.UIMap.AllAssertOpenPrep();
            // this.UIMap.AssertAllFileOpen();
            // this.UIMap.SprdAssertOpenPrep();
            // this.UIMap.AssertSprdOpen();


            //this.UIMap.AssertAllOpen();
            //this.UIMap.AssertOpenMethod();
            //this.UIMap.CloseForOpen();
            //this.UIMap.OpeningFiles();
            //this.UIMap.AssertOpeningFiles();
            //this.UIMap.OpenFunction();
            //this.UIMap.AssertOpenFunction();
            //this.UIMap.FileOpening();
            //this.UIMap.AssertFileOpen();
            //this.UIMap.OpenFunctions();
            //this.UIMap.AssertOpen();



        }

        #region Additional test attributes

        // You can use the following additional attributes as you write your tests:

        ////Use TestInitialize to run code before running each test 
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{        
        //    // To generate code for this test, select "Generate Code for Coded UI Test" from the shortcut menu and select one of the menu items.
        //}

        ////Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{        
        //    // To generate code for this test, select "Generate Code for Coded UI Test" from the shortcut menu and select one of the menu items.
        //}

        #endregion

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }
        private TestContext testContextInstance;

        public UIMap UIMap
        {
            get
            {
                if ((this.map == null))
                {
                    this.map = new UIMap();
                }

                return this.map;
            }
        }

        private UIMap map;
    }
}
