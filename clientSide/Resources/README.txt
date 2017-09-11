Alex Eakle, Brandon Butterbaugh 11/2/2016

Versions:
PS2.v1
PS3.v2
PS4.v2
PS5.v2
PS6.V1

Grader Notes:
******
to get full test coverage you need to deleate AllTest and UITest if test has been run before
******
No bytes were intentionally harmed durring the development of this project.

problems: 
	1. to many to count.

log (document) of requirements:
1. Variables in the formula class are valid as long as they consist of a letter or underscore followed by zero or more letters, underscores, or digits (as defined in PS3).

2. Variables for a Spreadsheet are only valid if they are one or more letters followed by one or more digits (numbers).

3. IsValid delegate may have even further contraints on what a valid variable is.

4. code should use the given IsValid function when calls are made to such functions as: GetCellContents; The spreadsheet code should also provide this same IsValid function to any formula object it creates.

5. There are now places where you might need to check a variable's validity: (o) does it pass a "loose" tokenizer definition of a variable; (o) does a variable pass the Spreadsheet variable test, 
	(o) does the variable pass the "outside developers" IsValid test.


/////// valid names, normalize, constructor for AbstractSpreadsheet ////////////////////////////////

6. a string is considered a valid cell name if and only if:
	  The string starts with one or more letters and is followed by one or more numbers.
	  The (application programmer's) IsValid function should be called only for variable strings that are valid first by (1) above.

7. The default normalize function should do nothing to a variable name.
8. The user of the spreadsheet can provide a different function that puts variables into (what the application programmer thinks of as) a standard form. For example, it might convert all the letters in a name to upper case.

9. when a valid cell name 'n' comes in as a parameter, you should replace it with Normalize(n).

10. when a string representing a fomula comes in as a parameter, you should replace each cell name 'n' that it contains with Normalize(n).

11.  the AbstractSpreadsheet constructor should propagate the "outside" world's validators/normalizers to the formula objects.


/////// Spreadsheet Versions////////////////////////////////

11.5 From now on, as you create and then save spreadsheets to disk, you will have to "version" the spreadsheet 
	(much the same idea as how we are using GIT, but to be clear we are not using GIT to version our spreadsheets themselves (just the code from our project)).

Our spreadsheets could eventually use a versioning scheme such as: "1" or "1.1", but could also be something like: "first version", or, as we will start with: "default".

/////// New spreadsheet constructors////////////////////////////////

12. AbstractSpreadsheet now provides a three-argument constructor that your Spreadsheet constructors will need to use.
13. Your zero-argument constructor should create an empty spreadsheet that imposes no extra validity conditions, normalizes every cell name to itself, and has version "default".
14. You should add a three-argument constructor to the Spreadsheet class. Just like the zero-argument constructor, it should create an empty spreadsheet. However, 
	it should allow the user to provide a validity delegate (first parameter), a normalization delegate (second parameter), and a version (third parameter).
15. Write four-argument constructor to the Spreadsheet class. It should allow the user to provide a string representing a path to a file (first parameter), 
	a validity delegate (second parameter), a normalization delegate (third parameter), and a version (fourth parameter). 
	It should read a saved spreadsheet from a file (see the Save method) and use it to construct a new spreadsheet. The new spreadsheet should use the provided validity delegate, normalization delegate, and version.


/////// Error Checking/Handling ////////////////////////////////

16. If anything goes wrong when reading the file, the constructor should throw a SpreadsheetReadWriteException with an explanatory message.
	. If the version of the saved spreadsheet does not match the version parameter provided to the constructor, an exception should be thrown.
	. If any of the names contained in the saved spreadsheet are invalid, an exception should be thrown.
	. If any invalid formulas or circular dependencies are encountered, an exception should be thrown.
	. If there are any problems opening, reading, or closing the file, an exception should be thrown.
	. There are no doubt other things that can go wrong and should be handled appropriately.

/////// Set Contents Of Cell ////////////////////////////////

17. There is a new abstract method SetContentsOfCell. The exisiting SetCellContents methods are now protected. This single function will determine if the given string is a double, formula, or string; 
	it will then call the appropriate SetCellContents function.
	(Note: formulas are now defined by the standard Excel method of placing an = (equals sign) in front of the text.)


/////// new methods and functions ////////////////////////////////

18. There is a new abstract method GetCellValue. This function gets the value as opposed to he contents of the cell

19. There is a new abstract property Changed. This class variable reflects if the spreadsheet has been modified after construction. 
	Additionally, if the spreadsheet is saved, it is no longer "changed" (until another change is made).

20. there is a new abstract method Save. You will save the spreadsheet as an XML file on disk.

21. There is a new abstract method GetSavedVersion. This function will look at a previously saved file and return the version information. 
	(Note: this should really be a static method, but C# does not allow abstract classes to define static methods.)



information that will help you:
depenency graphy- 
(s,t) T depends of S

dents: gives a list of items that depend on current "cell"
dees: gives a list of items that current "cell" depends on

SetCellContents- 
when SetCellContents has to deal with formulas it has to check if there was a formula already in the cell and remove the links between the the cells 
that the current "cell" depended on and replace them with links to the new cells that are variables in the formula.
if there is a circular dependency, from the new formula you need to remove the links that you created in the dependency graph and revert back to the old 
formula and its links. Hopefully this helps.