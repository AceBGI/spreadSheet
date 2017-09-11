// Alex Eakle
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpreadsheetUtilities;
using System.Text.RegularExpressions;
using System.Xml;
using System.IO;
using System.Xml.Linq;
using System.Xml.XPath;

namespace SS
{

    public class Spreadsheet  : AbstractSpreadsheet
    {
        private Dictionary<string, Cell> cellsDictionary = new Dictionary<string, Cell>();
        private DependencyGraph dependencyGraph = new DependencyGraph();

        /// <summary>
        /// Constructs an abstract spreadsheet by recording its variable validity test,
        /// its normalization method, and its version information.  The variable validity
        /// test is used throughout to determine whether a string that consists of one or
        /// more letters followed by one or more digits is a valid cell name.  The variable
        /// equality test should be used thoughout to determine whether two variables are
        /// equal.
        /// </summary>
        /// <param name="isValid"></param>
        /// <param name="normalize"></param>
        /// <param name="version"></param>
        public Spreadsheet(Func<string, bool> isValid, Func<string, string> normalize, string version) : base(isValid, normalize, version)
        {
            Version = version;
        }

        /// <summary>
        /// Your zero-argument constructor should create an empty spreadsheet that imposes no extra validity conditions, 
        /// normalizes every cell name to itself, and has version "default". 

        /// </summary>
        public Spreadsheet() : this (s=> true, n=> n, "default")
        {

        }

        public Spreadsheet(string filename, Func<string, bool> isValid, Func<string, string> normalize, string version) : base(isValid, normalize, version)
        {

            //   It should read a saved spreadsheet from a file (see the Save method) and use it to construct a new spreadsheet. 
            // create method that load in a xml file and then create a spreadsheet of cells from xml file.
            Load(filename);
            // loads information from namesContents dictionary into the SetContentsOfCell method.
            foreach (var key in namesContents.Keys)
            {
                SetContentsOfCell(key, namesContents[key]);
            }
            Version = version;

        }

        /// <summary>
        /// Is a internal method that finds out if your cells name is valid. or if the variable name is valid.
        /// Variables for a Spreadsheet are only valid if they are one or more letters followed by one or more digits (numbers).
        /// cell names are are valid if it starts with one or more letters and is followed by one or more numbers.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool isValid(string name)
        {
            bool isname = Regex.IsMatch(name, "^[a-zA-Z]+[0-9]+$");
            if (isname == true)
                return true;
            return false;
        }

        /// <summary>
        /// If name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, returns the contents (as opposed to the value) of the named cell.  The return
        /// value should be either a string, a double, or a Formula.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public override object GetCellContents(string name)
        {
            // change name to the normalized name, so that you get the corect cell
            string cellName = Normalize(name);
            if (cellName == null || isValid(cellName) == false)
            {
                throw new InvalidNameException();
            }
            else if (!(cellsDictionary.ContainsKey(cellName)))
            {
                return "";
            }
            if (cellsDictionary[cellName].Content.GetType() == typeof(Formula))
            {
                return "=" + cellsDictionary[cellName].Content;
            }

            return cellsDictionary[cellName].Content;
        }

        /// <summary>
        /// Enumerates the names of all the non-empty cells in the spreadsheet.
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<string> GetNamesOfAllNonemptyCells()
        {
            return cellsDictionary.Keys;
        }

        /// <summary>
        /// If the formula parameter is null, throws an ArgumentNullException.
        /// 
        /// Otherwise, if name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, if changing the contents of the named cell to be the formula would cause a 
        /// circular dependency, throws a CircularException.  (No change is made to the spreadsheet.)
        /// 
        /// Otherwise, the contents of the named cell becomes formula.  The method returns a
        /// Set consisting of name plus the names of all other cells whose value depends,
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="formula"></param>
        /// <returns></returns>
        protected override ISet<string> SetCellContents(string name, Formula formula)
        {
            HashSet<string> set = new HashSet<string>();

            if (formula == null)
                throw new ArgumentNullException();
            else if (name == null || isValid(name) == false)
                throw new InvalidNameException();
            // if the cell does not allready exsist add the variables from the formula as dependents, look through the formual and see if the variables cause a circular dependency if not add cells. 
            // then add the cell to the dictionary
            else if ( !(cellsDictionary.ContainsKey(name)) )
            {
                try
                {
                    // add new dependency links from formula then add all variable that need to be recalculated to the hashset = set
                    addLinks(formula, name);
                    addToSet(set, name);

                    cellsDictionary.Add(name, new Cell(formula));
                }
                // if there is a circularExeption remove dependen links and throw CircularException error
                catch (Exception)
                {
                    removeDepentents(name);
                    throw new CircularException();
                }               
            }
            //  if the cell allready exsist add the variables from the formula as dependents, look through the formual and see if the variables cause a circular dependency if not add cells
            else if (cellsDictionary.ContainsKey(name))
            {
                // does the exsisting formula have any variables if it does save the variables and if their is a circularExeption return depedencies back to the original depedences.
                if (formulaHasVariables(name, cellsDictionary[name].Content))
                {
                   
                    try
                    {
                        // remove old dependents then add new dependency links from formula then add all variable that need to be recalculated to the hashset = set
                        removeDepentents(name);
                        addLinks(formula, name);
                        addToSet(set, name);

                        cellsDictionary[name] = new Cell(formula);
                    }
                    // if there is a circularExeption remove dependen links and throwCircularException error
                    catch (Exception)
                    {
                        removeDepentents(name);
                        Formula f = (Formula)cellsDictionary[name].Content;
                        addLinks(f, name);
                        throw new CircularException();
                    }
                }
                else
                {
                    try
                    {
                        // add new dependency links from formula then add all variable that need to be recalculated to the hashset = set
                        addLinks(formula, name);
                        addToSet(set, name);

                        cellsDictionary[name] = new Cell(formula);
                    }
                    // if there is a circularExeption remove dependen links and throw CircularException error
                    catch (Exception)
                    {
                        removeDepentents(name);
                        throw new CircularException();
                    }
                }
            }
            set.Add(name);

            // recalculate list of items that has been changed
            foreach (var item in set)
            {
                Calculate(item, cellsDictionary[item].Content, lookup);
            }

            return set;
        }

        /// <summary>
        ///  checks to see if formula has any variables
        /// </summary>
        /// <param name="name"></param>
        /// <param name="formula"></param>
        /// <returns></returns>
        private bool formulaHasVariables(string name, object formula)
        {
            int count = 0;

            if (formula is Formula)
            {
                Formula f = (Formula)formula;
                foreach (var item in f.GetVariables() )
                {
                    count++;
                }

                if (count > 0)
                    return true;
            }  
            return false;
        }

        /// <summary>
        ///  remove dependent links Or in other words it takes all cells that need to happen first and removes the link that they have with the give cell "name"
        /// </summary>
        /// <param name="name"></param>
        private void removeDepentents(string name)
        {
            // get a list of all cells that cell "name" depends on
            List<string> dees = new List<string>();
            foreach (var item in dependencyGraph.GetDependees(name))
            {
                dees.Add(item);
            }

            // remove links to cells that cell "name" depends on.
            foreach (var d in dees)
            {
                dependencyGraph.RemoveDependency(d, name);
            }
        }

        /// <summary>
        /// Get all of the variables from the formula and then add those varaibles as dependents for the cell.
        /// the cell "name" depends on the variables of formula. the variable from the formula need to be calculated before name is calculated
        /// </summary>
        /// <param name="formula"></param>
        /// <param name="name"></param>
        private void addLinks (Formula formula, string name)
        {
            // add links for dependents
            List<string> formulaVariables = new List<string>(formula.GetVariables());
            foreach (string v in formulaVariables)
            {
                dependencyGraph.AddDependency(v, name);
            }
        }

        /// <summary>
        /// add all variable of NAME that need to be recalculated to the hashset = SET
        /// </summary>
        /// <param name="set"></param>
        /// <param name="name"></param>
        private void addToSet( HashSet<string> set, string name)
        {
            foreach (var item in GetCellsToRecalculate(name))
            {
                set.Add(item);
            }
        }

        /// <summary>
        ///  /// If text is null, throws an ArgumentNullException.
        /// 
        /// Otherwise, if name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, the contents of the named cell becomes text.  The method returns a
        /// set consisting of name plus the names of all other cells whose value depends, 
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        protected override ISet<string> SetCellContents(string name, string text)
        {
            HashSet<string> set = new HashSet<string>();

            if (text == null)
                throw new ArgumentNullException();
            else if (name == null || isValid(name) == false)
                throw new InvalidNameException();
            // if cell dosn't already exsists set cell to equal text and set all veriables that need to be recalculated to hashset<string> set
            else if (!(cellsDictionary.ContainsKey(name)))
            {
                // if text is "" return empty set and do nothing. 
                if (text == "")
                {
                    return set;
                }
                //add all variable that need to be recalculated to the hashset = set
                addToSet(set, name);
                cellsDictionary.Add(name, new Cell(text));
            }
            // if cell already exsists set cell to equal text and set all veriables that need to be recalculated to hashset<string> set
            else if (cellsDictionary.ContainsKey(name))
            {
                // if cell already has a formula with variables, remove dependees of cell "name"
                if (formulaHasVariables(name, cellsDictionary[name].Content))
                {
                    removeDepentents(name);
                }
                //add all variable that need to be recalculated to the hashset = set
                addToSet(set, name);
                cellsDictionary[name] = new Cell(text);
            }
            set.Add(name);

            // recalculate list of items that has been changed
            foreach (var item in set)
            {
                Calculate(item, cellsDictionary[item].Content, lookup);
            }

            return set;
        }

        /// <summary>
        /// If name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, the contents of the named cell becomes number.  The method returns a
        /// set consisting of name plus the names of all other cells whose value depends, 
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        protected override ISet<string> SetCellContents(string name, double number)
        {
            HashSet<string> set = new HashSet<string>();

            if (name == null || isValid(name) == false)
                throw new InvalidNameException();
            // if cell dosn't already exsists set cell to equal number and set all veriables that need to be recalculated to hashset<string> set
            else if (!(cellsDictionary.ContainsKey(name)))
            {
                //add all variable that need to be recalculated to the hashset = set
                addToSet(set, name);
                cellsDictionary.Add(name, new Cell(number));
            }
            // if cell already exsists set cell to equal number and set all veriables that need to be recalculated to hashset<string> set
            else if (cellsDictionary.ContainsKey(name))
            {
                // if cell already has a formula with variables, remove dependees of cell "name"
                if (formulaHasVariables(name, cellsDictionary[name].Content))
                {
                    removeDepentents(name);
                }
                //add all variable that need to be recalculated to the hashset = set
                addToSet(set, name);
                cellsDictionary[name] = new Cell(number);
            }
            set.Add(name);

            // recalculate list of items that has been changed
            foreach (var item in set)
            {
                Calculate(item, cellsDictionary[item].Content, lookup);
            }

            return set;
        }

        /// <summary>
        /// If name is null, throws an ArgumentNullException.
        /// 
        /// Otherwise, if name isn't a valid cell name, throws an InvalidNameException.
        /// 
        /// Otherwise, returns an enumeration, without duplicates, of the names of all cells whose
        /// values depend directly on the value of the named cell.  In other words, returns
        /// an enumeration, without duplicates, of the names of all cells that contain
        /// formulas containing name.
        /// 
        /// For example, suppose that
        /// A1 contains 3
        /// B1 contains the formula A1 * A1
        /// C1 contains the formula B1 + A1
        /// D1 contains the formula B1 - C1
        /// The direct dependents of A1 are B1 and C1
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        protected override IEnumerable<string> GetDirectDependents(string name)
        {
            if (name == null)
                throw new ArgumentNullException();
            else if (isValid(name) == false)
                throw new InvalidNameException();
            return dependencyGraph.GetDependents(name);
        }

        /// <summary>
        /// True if this spreadsheet has been modified since it was created or saved                  
        /// (whichever happened most recently); false otherwise.
        /// </summary>
        public override bool Changed
        {
            // how do I find out if the spreadsheet has been modified
            // *****************************************************************************
            get;


            protected set;
        }

        /// <summary>
        /// Returns the version information of the spreadsheet saved in the named file.
        /// If there are any problems opening, reading, or closing the file, the method
        /// should throw a SpreadsheetReadWriteException with an explanatory message.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public override string GetSavedVersion(string filename)
        {
            string saved = "default";
            // List<string> attributes = new List<string>();
            try
            {
                // load up the readTest.xml file using XDocument "readTest.xml"
                XDocument xd;
                xd = XDocument.Load(filename);

                // do a xpath select elements of spreadsheet
                // this gives you a list of nodes (//) is short hand for start at the root and then find any employ node underneath that
                var spreadsheet = xd.XPathSelectElements("//spreadsheet");
                foreach(XAttribute xn in spreadsheet.Attributes())
                {
                    saved = xn.Value;
                }
                return saved;
            }
            // if file is unable to be load throw a SpreadsheetReadWriteException
            catch (FileNotFoundException)
            {
                throw new SpreadsheetReadWriteException("file was not found, or is unable to load file");
            }
        }

        /// <summary>
        /// Writes the contents of this spreadsheet to the named file using an XML format.
        /// The XML elements should be structured as follows:
        /// 
        /// <spreadsheet version="version information goes here">
        /// 
        /// <cell>
        /// <name>
        /// cell name goes here
        /// </name>
        /// <contents>
        /// cell contents goes here
        /// </contents>    
        /// </cell>
        /// 
        /// </spreadsheet>
        /// 
        /// There should be one cell element for each non-empty cell in the spreadsheet.  
        /// If the cell contains a string, it should be written as the contents.  
        /// If the cell contains a double d, d.ToString() should be written as the contents.  
        /// If the cell contains a Formula f, f.ToString() with "=" prepended should be written as the contents.
        /// 
        /// If there are any problems opening, writing, or closing the file, the method should throw a
        /// SpreadsheetReadWriteException with an explanatory message.
        /// </summary>
        /// <param name="filename"></param>
        public override void Save(string filename)
        {
            XDocument xd;
            XElement root;
            XElement child;
            XElement child2;

            // create a XDocument in memory and use overloads to specifiy information
            xd =
                new XDocument(
                    new XDeclaration("1.0", "utf-8", "yes"),
                    new XComment("List of spreadsheet cells"),
                    new XElement("spreadsheet", new XAttribute("version", Version) ));

            // create a root node and then set child to that root
            root = xd.XPathSelectElement("//spreadsheet");

            // take cellsDictionary and loop through its values and add them as a child2 to child, which is a child of root.
            foreach (var key in cellsDictionary.Keys)
            {
                child = new XElement("cell");
                root.Add(child);
                child2 = new XElement("name", key);
                child.Add(child2);
                if (cellsDictionary[key].Content.GetType() == typeof(Formula))
                {
                    child2 = new XElement("contents", "=" + cellsDictionary[key].Content);
                }else
                {
                    child2 = new XElement("contents", cellsDictionary[key].Content);
                }
                
                child.Add(child2);
            }

            // save XDocument to a stream which is a xml document.
            try
            {
                xd.Save(filename);
            }
            catch (Exception)
            {
                throw new SpreadsheetReadWriteException("the file path could not be found");
            }
            
            Changed = false;

        }

        public Dictionary<string, string> namesContents = new Dictionary<string, string>();
        /// <summary>
        /// Loads in a document and adds its elements to the namesContents dictionary
        /// </summary>
        public void Load(string filename)
        {
            try
            {
                // load up the readTest.xml file using XDocument "readTest.xml"
                XDocument xd;
                try
                {
                    xd = XDocument.Load(filename);
                }
                catch (Exception)
                {
                    throw new SpreadsheetReadWriteException("path name could not be found");
                }
                

                // do a xpath select elements of cell
                // this gives you a list of nodes (//) is short hand for start at the root and then find any employ node underneath that
                var list = xd.XPathSelectElements("//cell");
                // take a list of nodes and add them to the namesContents dictionary
                foreach (XElement xn in list)
                {
                    namesContents.Add(xn.Element("name").Value, xn.Element("contents").Value);
                }
            }
            // if file is unable to be load throw a SpreadsheetReadWriteException
            catch (FileNotFoundException)
            {
                throw new SpreadsheetReadWriteException("file was not found, unable to load file");
            }

            // if the version from the loaded file does not match the version provided in the constructor throw a exception.
            string fileVersion = GetSavedVersion(filename);
            if (fileVersion != Version)
                throw new SpreadsheetReadWriteException("the version of the saved spreadsheet does not match the version parameter provided to the constructor");

            foreach (var key in namesContents.Keys)
            {
                if (isValid(key) == false)
                    throw new SpreadsheetReadWriteException("The cells name contained in the saved spreadsheet are invalid");
            }
        }

        // ********************************************************************************************************************
        // **** look at this and make sure that the GetCellValue and Calculate and lookup work


        /// <summary>
        /// If name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, returns the value (as opposed to the contents) of the named cell.  The return
        /// value should be either a string, a double, or a SpreadsheetUtilities.FormulaError.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public override object GetCellValue(string name)
        {

            if (name == null || isValid(name) == false)
                throw new InvalidNameException();
            try
            {
                return cellsDictionary[name].Value;
            }
            catch (Exception)
            {
                return "";
            }
            
        }

        /// <summary>
        /// look and see if content is a formula if it is evalutae content using Evaluate(lookup) set the value to answer and return that answer
        /// </summary>
        /// <param name="name"></param>
        /// <param name="content"></param>
        /// <param name="lookup"></param>
        /// <returns></returns>
        private object Calculate(string name, object content, Func<string, double> lookup)
        {
            // look and see if content is a formula else return the string or double
            if (content is Formula)
            {
                // evalutae content using Evaluate(lookup) set the value to answer and return.
                Formula f = (Formula)content;
                object answer = f.Evaluate(lookup);
                // set the value of that cell to answer so that you have it for later.
                cellsDictionary[name].Value = answer;
                return answer;
            }
            if (content is string)
                cellsDictionary[name].Value = (string)content;
            if (content is double)
                cellsDictionary[name].Value = (double)content;

            return content;
        }

        /// <summary>
        /// look up the variables value
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public double lookup(string name)
        {
            double value;
            if (cellsDictionary.ContainsKey(name) == false)
                throw new ArgumentException("Unknown variable");
            else if (Double.TryParse(GetCellValue(name).ToString(), out value))
                return value;
            return 0;
        }

        /// <summary>
        /// If content is null, throws an ArgumentNullException.
        /// 
        /// Otherwise, if name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, if content parses as a double, the contents of the named
        /// cell becomes that double.
        /// 
        /// Otherwise, if content begins with the character '=', an attempt is made
        /// to parse the remainder of content into a Formula f using the Formula
        /// constructor.  There are then three possibilities:
        /// 
        /// *************************************************************************************
        ///   (1) If the remainder of content cannot be parsed into a Formula, a 
        ///       SpreadsheetUtilities.FormulaFormatException is thrown.
        ///       
        ///   (2) Otherwise, if changing the contents of the named cell to be f
        ///       would cause a circular dependency, a CircularException is thrown.
        ///       
        ///   (3) Otherwise, the contents of the named cell becomes f.
        /// 
        /// Otherwise, the contents of the named cell becomes content.
        /// 
        /// If an exception is not thrown, the method returns a set consisting of
        /// name plus the names of all other cells whose value depends, directly
        /// or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public override ISet<string> SetContentsOfCell(string name, string content)
        {
            double dNumber;
            ISet<string> set;

            if (content == null)
                throw new ArgumentNullException();
            else if (name == null || IsValid(name) == false)
                throw new InvalidNameException();
            
            // when a valid cell name 'n' comes in as a parameter, you should replace it with Normalize(n). 
            name = Normalize(name);

            // check if it is a double string or formula. call the approperate SetCellContents and set changed to true.
            if (Double.TryParse(content, out dNumber))
            {
                set = SetCellContents(name, dNumber);
                Changed = true;
                return set; 
            }
            else if (content.StartsWith("="))
            {
                // remove the "=" char from the string.
                string formulaContent = content.TrimStart('=');

                // parse content into a formula
                Formula f = new Formula(formulaContent, Normalize, IsValid);
                set = SetCellContents(name, f);
                Changed = true;
                return set;
            }
            set = SetCellContents(name, content);
            Changed = true;
            return set;
        }
    }

    /// <summary>
    /// A class the has 3 constructors that inisulize my class with a defult value
    /// </summary>
    public class Cell
    {
        /// content is a object that will be set to be ethier a string, double, forumla
        public object Content { get; protected set; }
        public object Value { get; set; }

        public Cell(string s)
        {
            this.Content = s;
        }

        public Cell(double d)
        {
            this.Content = d;
        }

        public Cell(Formula f)
        {
            this.Content = f;
        }
    }
}
