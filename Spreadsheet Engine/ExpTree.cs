//Drew Miller 11382134

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//work on splitexpression, isvalidexpression, createtree, and highest precedence

namespace CptS322
{
    //tree for creating algebraic expressions.
    //returns 0 if expresson is not valid.
    public class ExpTree
    {
        //Basic member variables. Dictionary for mapping variable names
        //to values, a string to store the epxression, and hardcoded accepted operators.
        private Dictionary<string, double> _variables = new Dictionary<string, double>();
        private string _expression;
        private Dictionary<char, int> _precedence = new Dictionary<char, int>();

        //creates a menu that takes an inteer 1 through 5 and executes commands accordingly
        //else just tosses away the user's input
        public void Menu1()
        {
            bool run = true;
            while (run)
            {
                Console.WriteLine("Menu (Current expression=\"" + _expression + "\")");

                //writes all the defined variables to the screen.
                writeVariables();

                Console.WriteLine("  1 = Enter a new expresion");
                Console.WriteLine("  2 = Set a variable Value");
                Console.WriteLine("  3 = Remove a variable Value");
                Console.WriteLine("  4 = Evaluate tree");
                Console.WriteLine("  5 = Quit");

                string userInput = Console.ReadLine();

                bool pause = false;

                if(userInput == "1")
                {
                    Console.Write("Enter a new expression: ");
                    string expression = Console.ReadLine();
                    changeExpression(expression);
                }

                else if (userInput == "2")
                {
                    Console.Write("Enter variable name: ");
                    string name = Console.ReadLine();
                    Console.Write("Enter variable value: ");
                    string value = Console.ReadLine();

                    bool isN = isName(name);
                    bool isD = isDouble(value);

                    if (isD && isN)
                    {
                        SetVar(name, toDouble(value));
                        Console.WriteLine(name + " : " + toDouble(value));
                    }

                    if(!isN)
                    {
                        Console.WriteLine("Not a valid variable name.");
                        pause = true;
                    }

                    if (!isD)
                    {
                        Console.WriteLine("Not a valid variable value.");
                        pause = true;
                    }
                }

                else if (userInput == "3")
                {
                    Console.Write("Enter a variable to remove: ");
                    string varName = Console.ReadLine();
                    RemoveVar(varName);
                }

                else if (userInput == "4")
                {
                    //if it is a valid expression, print it
                    if (isValidExpression(splitExpression()))
                    {
                        Console.WriteLine("Evaluation " + Eval());
                        pause = true;
                    }

                    else
                    {
                        Console.WriteLine("Not a valid expression");
                        pause = true;
                    }
                }

                else if (userInput == "5")
                {
                    run = false;
                }

                if (pause)
                {
                    Console.Write("Press any key to continue . . .");
                    Console.ReadKey();
                }

                Console.Clear();
            }
        }

        //menu that YOU will see. Has less console comments which
        //I had implemented for my own visibility of code.
        public void Menu2()
        {
            bool run = true;
            while (run)
            {
                Console.WriteLine("Menu (Current expression=\"" + _expression + "\")");
                Console.WriteLine("  1 = Enter a new expresion");
                Console.WriteLine("  2 = Set a variable Value");
                Console.WriteLine("  3 = Evaluate tree");
                Console.WriteLine("  4 = Quit");

                string userInput = Console.ReadLine();

                if (userInput == "1")
                {
                    Console.Write("Enter a new expression: ");
                    string expression = Console.ReadLine();
                    changeExpression(expression);
                }

                else if (userInput == "2")
                {
                    Console.Write("Enter variable name: ");
                    string name = Console.ReadLine();
                    Console.Write("Enter variable value: ");
                    string value = Console.ReadLine();

                    bool isN = isName(name);
                    bool isD = isDouble(value);

                    if (isD && isN)
                    {
                        SetVar(name, toDouble(value));
                        Console.WriteLine(name + " : " + toDouble(value));
                    }

                    if (!isN)
                    {
                        Console.WriteLine("Not a valid variable name.");
                    }

                    if (!isD)
                    {
                        Console.WriteLine("Not a valid variable value.");
                    }
                }

                else if (userInput == "3")
                {
                    if (isValidExpression(splitExpression()))
                    {
                        Console.WriteLine("Evaluation " + Eval());
                    }

                    else
                    {
                        Console.WriteLine("Not a valid expression");
                    }
                }

                else if (userInput == "4")
                {
                    run = false;
                }
            }
        }

        //uses to change expression
        public void changeExpression(string expression)
        {
            _expression = expression;
            _variables = new Dictionary<string, double>();
        }

        //writes all the variables in a line for for user information
        void writeVariables()
        {
            //writes all the defined variables to the screen.
            if (_variables.Count != 0)
            {
                Console.Write("Variables Defined = ");
                var keys = _variables.Keys;

                //keeps track of the iteration through the list of strings.
                //if we are at the second to last, do not add a comma.
                int i = 0;
                foreach (string s in keys)
                {
                    Console.Write(s);
                    i++;

                    if (i < keys.Count)
                    {
                        Console.Write(", ");
                    }
                }

                Console.Write("\n");
            }

            else { Console.WriteLine("No variables defined"); }
        }

        //initializes expression tree
        public ExpTree(string expression, Dictionary<string, double> variables)
        {
            _expression = expression;
            _variables = variables;

            _precedence.Add('+', 1);
            _precedence.Add('-', 1);
            _precedence.Add('*', 2);
            _precedence.Add('/', 2);
            _precedence.Add('(', 0);
            _precedence.Add(')', 0);
        }

        //if varName is not in dictionary, create a new pair
        //else if already existing, override the previous value.
        public void SetVar(string varName, double varValue)
        {
            if (!_variables.ContainsKey(varName))
            {
                _variables.Add(varName, varValue);
            }
            else
            {
                _variables[varName] = varValue;
            }
        }

        //if varName is a key in the dictionary, remove if and its value from the dictionary
        public void RemoveVar(string varName)
        {
            if (_variables.ContainsKey(varName))
            {
                _variables.Remove(varName);
            }
        }

        //evaluates the expression after creating a tree of nodes
        //WILL RETURN 0 IF NOT VALID EXPRESSION AND/OR A VARIABLE IS NOT DEFINED.
        //if you try to use an undefined variable, the parser will not know it is a variable and
        //will count the expression as invalid since it will not contain only constants, variables, and/or operators.
        public double Eval()
        {
            double evaluation = 0;

            //splits our expression into a list of strings
            List<string> split = splitExpression();

            if (isValidExpression(split))
            {
                //create tree of nodes
                Node root = createTree(split);

                //evaluates nodes by passing in root of tree
                evaluation = evaluate(root);
            }

            return evaluation;
        }

        void printTree(Node root)
        {
            if(root is opNode)
            {
                opNode myRoot = (opNode)root;

                if (myRoot.Left != null)
                {
                    Console.Write("(");
                    printTree(myRoot.Left);
                }

                Console.WriteLine(myRoot.Data);

                if(myRoot.Right != null)
                {
                    printTree(myRoot.Right);
                    Console.Write(")");
                }
            }

            else
            {
                Console.Write(" " + root.Data + " " );
            }
        }

        //splits the expression into a list of variables and operators
        //make it more leniant on variables and make it accept integers
        public List<string> splitExpression()
        {
            List<string> mySplit = new List<string>();
            StringBuilder sb = new StringBuilder();
            bool valid = true;
            
            for(int i = 0; i < _expression.Length; i++)
            {
                //if the character at the index is not a number, letter, or expression, it is an invalid character
                if(!((isNumber(_expression[i])) || (isOperator(_expression[i])) || (isLetter(_expression[i])) || (_expression[i] == ' ')))
                {
                    valid = false;
                }

                if(_expression[i] == ' ')
                {
                    //does nothing, passes over spaces
                }

                //until we hit an operator, we will append all letters and numbers
                //to collect all of the variables in the expression.
                else if (i <= _expression.Length)
                {
                    if (isNumber(_expression[i]) || isLetter(_expression[i]))
                    {
                        sb.Append(_expression[i]);
                    }
                    //if it is an operator, create string and add it to our list of strings.
                    //Also add the correct operator to the list of strings.
                    else if (_precedence.ContainsKey(_expression[i]))
                    {
                        //if the string is not empty
                        if(sb.Length > 0)
                        {
                            mySplit.Add(sb.ToString());
                        }

                        mySplit.Add(_expression[i].ToString());
                        sb = new StringBuilder();
                    }
                }
            }

            //if there are characters still left in the stringbuilder after exiting
            if (sb.Length > 0)
            {
                //discharges characters into mysplit at end.
                mySplit.Add(sb.ToString());
            }

            //if we have a character that is not alphanumeric or a valid operator
            if(!valid)
            {
                mySplit = new List<string>();
                mySplit.Add(_expression);
            }

            return mySplit;
        }

        //checks list of strings to see if it is valid
        bool isValidExpression(List<string> mySplit)
        {
            bool valid = true;
            //used to keep track of whether or not we should be reading a value or operator
            bool counter = true;
            int paranthesis = 0;

            foreach (string s in mySplit)
            {
                //takes care of the paranthesis now, so we don't count them
                //in the counter for operators
                if(s == "(")
                {
                    paranthesis++;
                }

                else if (s == ")")
                {
                    paranthesis--;
                }

                //if counter is true we should have a name or double
                else if(counter && (isName(s) || isDouble(s)))
                {
                    counter = false;
                }

                //if counter is false, we should have an operator
                //set counter to true
                else if(!counter && (isOperator(s[0]) && (s.Length == 1)))
                {
                    counter = true;
                }

                else
                {
                    valid = false;
                    break;
                }
            }

            if((paranthesis != 0) || (counter))
            {
                valid = false;
            }

            return valid;
        }

        //checks list on strings to see if it is valid
        //Different overload
        public bool isValidExpression()
        {
            bool valid = true;
            var mySplit = splitExpression();
            //used to keep track of whether or not we should be reading a value or operator
            bool counter = true;
            int paranthesis = 0;

            foreach (string s in mySplit)
            {
                //takes care of the paranthesis now, so we don't count them
                //in the counter for operators
                if (s == "(")
                {
                    paranthesis++;
                }

                else if (s == ")")
                {
                    paranthesis--;
                }

                //if counter is true we should have a name or double
                else if (counter && (isName(s) || isDouble(s)))
                {
                    counter = false;
                }

                //if counter is false, we should have an operator
                //set counter to true
                else if (!counter && (isOperator(s[0]) && (s.Length == 1)))
                {
                    counter = true;
                }

                else
                {
                    valid = false;
                    break;
                }
            }

            if ((paranthesis != 0) || (counter))
            {
                valid = false;
            }

            return valid;
        }

        //creates a tree of nodes from a given list of strings
        Node createTree(List<string> expression)
        {
            Node myNode = null;

            //find highet lowest precedence operator,
            //pass all left and all right of that operator
            //recursively, and create a tree from the left and right
            //if no operator exists, return a const or var node.
            
            int index = lowestPrecedence(expression);

            //if it is -1, we have have only a variable (possibly nested), do not split it.
            //if index is not negative 1, we do encounter an operator.
            //split
            if (index != -1 && !(toParse(expression)))
            {
                var myOpNode = new opNode(expression[index]);

                List<string> left = expression.GetRange(0, lowestPrecedence(expression));
                List<string> right = expression.GetRange(lowestPrecedence(expression) + 1, expression.Count - lowestPrecedence(expression) - 1);


                myOpNode.Left = createTree(left);
                myOpNode.Right = createTree(right);

                myNode = myOpNode;
            }

            //if we have a single variable and doesn't need to be parsed
            else if(index == -1 && !toParse(expression))
            {
                //if we have a variable name
                if (isName(expression[0]))
                {
                    var myVar = new varNode(expression[0]);

                    if(_variables.ContainsKey(expression[0]))
                    {
                        myVar.Value = _variables[myVar.Data];
                    }

                    else { myVar.Value = 0; }

                    myNode = myVar;
                }

                //if we have a double
                else if (isDouble(expression[0]))
                {
                    var myConst = new constNode(expression[0]);
                    myNode = myConst;
                }
            }

            //else parse off the paranthesis
            else if(toParse(expression))
            {
                var myNewList = expression.GetRange(1, expression.Count - 2);
                myNode = createTree(myNewList);
            }

            return myNode;
        }

        //returns negative one if no operators in the expression. ex the expression is A1
        int lowestPrecedence(List<string> expression)
        {
            int index = -1;
            int currentPrecedence = -1;
            //used to indicate what level of paranthesis we are in
            int paranthesis = 0;

            for(int i = (expression.Count - 1); i >= 0; i--)
            {
                if (expression[i] == ")")
                {
                    paranthesis++;
                }

                if (expression[i] == "(")
                {
                    paranthesis--;
                }

                if (paranthesis == 0)
                {
                    //if we have an operator and we are outside of parathesis, that will be the highest operator
                    if (isOperator(expression[i][0]) && (expression[i].Length == 1) && (expression[i] != ")" && expression[i] != "("))
                    {
                        //if we encountered any operator, we take the first encountered as the lowest precedence.
                        if (currentPrecedence == -1)
                        {
                            index = i;
                            currentPrecedence = _precedence[expression[i][0]];
                        }

                        //if the precedence of the expression is lower than our current precedence, that is our new precedence
                        else if (_precedence[expression[i][0]] < currentPrecedence)
                        {
                            index = i;
                            currentPrecedence = _precedence[expression[i][0]];
                        }
                    }
                }
            }

            //if we didn't hit any operators, it could be because we only have a variable
            //BUT it could also be that we are nexted in parathesis

            if (index == -1)
            {
                //if we ARE nexted, parse them off
                if (toParse(expression))
                {

                    List<string> noParans = new List<string>();

                    //adds all elements besides the first and last,
                    //this parses off the paranthesis
                    for (int j = 1; j < expression.Count - 1; j++)
                    {
                        noParans.Add(expression[j]);
                    }

                    //if we get a return of negative one, then we have a variable nested in paranthesis
                    //let us know that even though there are parenthesis, there is only a variable in there
                    if (lowestPrecedence(noParans) == -1)
                    {
                        index = -1;
                    }



                    //adds one because we removed a paranthesis so we add one to make up for the index
                    //at the begininning being removed
                    else
                    {
                        index = lowestPrecedence(noParans) + 1;
                    }
                }
            }

            return index;
        }
        
        //used to determine if an expression is fully encompassed by paranthesis
        bool toParse(List<string> expression)
        {
            bool valid = true;
            int paranthesis = 0;

            if((expression[0] != "(") || (expression[expression.Count-1] != ")"))
            {
                valid = false;
            }

            for (int i = (expression.Count - 1); i >= 0; i--)
            {
                if(expression[i] == ")")
                {
                    paranthesis++;
                }

                if(expression[i] == "(")
                {
                    paranthesis--;
                }

                if ((paranthesis == 0) && (i != 0))
                {
                    valid = false;
                }
            }

            return valid;
        }

        //travels through a node and evaluates the tree
        //NOTE:
        //this version CAN take multiple expressions,
        //But does not take into account precedence of operators.
        double evaluate(Node root)
        {
            double evaluation = 0;

            if(root is opNode)
            {
                opNode myOp = (opNode)root;

                double lv = evaluate(myOp.Left);

                double rv = evaluate(myOp.Right);

                if(myOp.Data == "+")
                {
                    evaluation = lv + rv;
                }
                else if (myOp.Data == "-")
                {
                    evaluation = lv - rv;
                }
                else if (myOp.Data == "/")
                {
                    evaluation = lv / rv;
                }
                else if (myOp.Data == "*")
                {
                    evaluation = lv * rv;
                }
            }

            else if(root is constNode)
            {
                constNode myConst = (constNode)root;
                evaluation = myConst.Value;
            }

            else if(root is varNode)
            {
                 varNode myVar = (varNode)root;
                 evaluation = myVar.Value;
            }

            return evaluation;
        }

        //returns assigned value of variable
        //NOTE: DO use on doubles in the split expression because
        //while splitting we define the string doubles as a variable with
        //its double value as the dictionaries pair value
        double getValue(string key)
        {
            double value = 0;

            if(_variables.ContainsKey(key))
            {
                value = _variables[key];
            }

            return value;
        }

        //determines whether or not the character is an operator
        bool isOperator(char c)
        {
            bool isO = false;

            foreach(char o in _precedence.Keys)
            {
                if(o == c)
                {
                    isO = true;
                }
            }

            return isO;
        }

        //returns true if name is a valid variable name
        bool isName(string name)
        {
            bool valid = true;

            //if the first letter is not a valid letter, then the variable name is false
            if(!isLetter(name[0]))
            {
                valid = false;
            }

            //parse through the rest of the string
            foreach(char c in name.Substring(1))
            {
                //if the character is neither a number or letter, invalid character
                if(!isNumber(c))
                {
                    valid = false;
                }
            }

            if(valid)
            {
                if(toDouble(name.Substring(1)) > 50)
                {
                    valid = false;
                }
            }

            return valid;
        }

        //returns true if the string is a valid double
        bool isDouble(string value)
        {
            bool valid = true;

            foreach (char c in value)
            {
                if (!isNumber(c))
                {
                    valid = false;
                }
            }

            return valid;
        }

        //converts a valid double string to an actual double value and returns it
        double toDouble(string value)
        {
            double evaluation = 0;

            //does a secondary check to make sure the value
            //passed is indeed a double
            if (isDouble(value))
            {
                foreach (char c in value)
                {
                    evaluation = (evaluation * 10) + (c - 48);
                }
            }

           return evaluation;
        }

        //i was writing this code a lot and so i just made small fucntions
        bool isLetter(char c)
        {
            return (c >= 'A' && c <= 'Z')  || (c >= 'a' && c <= 'z');
        }

        //same, was writing this function a lot
        bool isNumber(char c)
        {
            return (c >= '0' && c <= '9');
        }
    }

    //base class node used to store data and create a tree.
    abstract class Node
    {
        private string _data;

        //basic constructor
        protected Node(string data)
        {
            _data = data;
        }

        public string Data
        {
            get { return _data; }
            set { _data = value; }
        }
    }

    //different classes of nodes
    internal class constNode : Node
    {
        private double _value;

        public constNode(string data) : base(data)
        {
            _value = toDouble(data);
        }

        public double Value
        {
            get { return _value; }
        }

        private double toDouble(string value)
        {
            double evaluation = 0;

            //does a secondary check to make sure the value
            //passed is indeed a double
            if (isDouble(value))
            {
                foreach (char c in value)
                {
                    evaluation = (evaluation * 10) + (c - 48);
                }
            }

            return evaluation;
        }

        private bool isDouble(string value)
        {
            bool valid = true;

            foreach (char c in value)
            {
                if (!isNumber(c))
                {
                    valid = false;
                }
            }

            return valid;
        }

        private bool isNumber(char c)
        {
            return (c >= '0' && c <= '9');
        }
    }

    internal class varNode : Node
    {
        private double _value;

        public varNode(string data) : base(data)
        {
            
        }

        //cannot set own value on construction because
        //we must interact with the definitions of variables
        //in the exptree class
        public double Value
        {
            get { return _value; }
            set { _value = value; }
        }
    }

    internal class opNode : Node
    {
        private Node _right;
        private Node _left;

        //cannot set own operator on construction
        //since the predefined accepted operators are
        //type cast in the exptree. Must interact with exptree
        public opNode(string data) : base(data)
        {

        }

        public Node Right
        {
            get { return _right; }
            set { _right = value; }
        }

        public Node Left
        {
            get { return _left; }
            set { _left = value; }
        }
    }
}
