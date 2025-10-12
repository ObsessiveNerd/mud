using Godot;
using Godot.Collections;
using System;
using System.IO;

using MenuContextLookup = System.Collections.Generic.Dictionary<string,IMenuContext>;

public interface IMenuContext
{
    void Start();
    IMenuContext Evaluate(string input, out string result);
}

public static class Globals
{
    public static Character CurrentCharacter;
}

public static class Paths
{
    public static string CharactersPath = "./Characters/";
}

public partial class InputParser : Node
{
    IMenuContext currentContext;
    public NetworkManager networkManager;

    MenuContextLookup menuActions = new MenuContextLookup()
    {
        { "create", new CharacterCreator() },
        { "load", new CharacterLoader() },
        { "list", new CharacterLister() },
        
    };

    public string ParseInput(TerminalContext context, string input)
    {
        GD.Print($"Parsing input: {input}");

        if (input == "help")
        {
            if (context == TerminalContext.Game)
                return ShowGameHelp();
            else
                return ShowMenuHelp();
        }

        if (input == "exit")
            currentContext = null;

        string result = "";
        if (currentContext != null)
            currentContext = currentContext.Evaluate(input, out result);
        else
        {
            if (context == TerminalContext.Menu)
            {
                var tokens = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (menuActions.ContainsKey(tokens[0]))
                {
                    currentContext = menuActions[tokens[0]];
                    currentContext.Start();
                    string parameters = "";
                    if (tokens.Length > 1)
                        parameters = string.Join(' ', tokens, 1, tokens.Length - 1);
                    currentContext = currentContext.Evaluate(parameters, out result);
                }

                if (tokens[0] == "server")
                {
                    networkManager.StartServer();
                    result = "Server started.";
                }
                else if (tokens[0] == "connect")
                {
                    string address = tokens.Length > 1 ? tokens[1] : "127.0.0.1";
                    networkManager.ConnectToServer(address);
                    result = "Connecting to server...";
                }
                else if (tokens[0] == "look")
                {
                    result += "Characters connected:\n";
                    foreach(var player in networkManager.GetAllCharacters().Values)
                    {
                        result += $"\t{player.Name} the {player.CharacterClass.ClassName}\n";
                    }
                }

            }
        }

        return result;
    }

    string ShowGameHelp()
    {
        return "List of commands:\n" +
                "\t\tgo [direction] - Move in a direction (north, south, east, west)\n" +
                "\t\tattack [enemy name] - Attack the enemy\n" +
                "\t\tlook - Look around\n" +
                "\t\ttake [item] - Take an item\n" +
                "\t\tuse [item] - Use an item\n" +
                "\t\tuse [item] on [name] - Use an item on something else in the room\n" +
                "\t\tdrop [item] - Drop an item\n" +
                "\t\tinventory - Show your inventory\n" +
                "\t\tstatus - Show your status\n" +
                "\t\thelp - Show this help message\n";
    }

    string ShowMenuHelp()
    {
        return "List of commands:\n" +
                "\t\tcreate character - create a new character\n" +
                "\t\tlist characters - list existing characters\n" +
                "\t\tload character [name] - load an existing character\n" +
                "\t\tdelete character [name] - delete an existing character\n" +
                "\t\tquit - exit the game\n" +
                "\t\thelp - Show this help message\n";
    }

    class CharacterLister : IMenuContext
    {
        public IMenuContext Evaluate(string input, out string result)
        {
            result = ListCharacters();
            return null;
        }

        public void Start()
        {
            //Do Nothing
        }

        public string ListCharacters()
        {
            string result = "";
            if (!Directory.Exists(Paths.CharactersPath))
            {
                result += "\tNo characters found.\n";
                return null;
            }

            var files = Directory.GetFiles(Paths.CharactersPath, "*.tres");
            foreach (var file in files)
            {
                var charName = Path.GetFileNameWithoutExtension(file);
                result += $"\t{charName}\n";
            }

            return result;
        }
    }

    class CharacterLoader : IMenuContext
    {
        public IMenuContext Evaluate(string input, out string result)
        {
            if (string.IsNullOrEmpty(input))
            {
                result = "You need to specify a character to load.\nExisting characters:\n";
                result += new CharacterLister().ListCharacters();
            }
            else
            {
                var character = ResourceLoader.Load<Character>($"{Paths.CharactersPath}/{input}.tres");
                Globals.CurrentCharacter = character;
                result = $"{character.Name} has been loaded\n";
                result += character.ToString();
            }

            return null; // Exit context
        }

        

        public void Start()
        {
            
        }
    }

    class CharacterCreator : IMenuContext
    {
        Character character;
        Class currentClassSelection;

        enum CharacterCreationStep
        {
            Start,
            Class,
            ClassConfirm,
            Name,
            Confirm
        }

        CharacterCreationStep creationStep = CharacterCreationStep.Start;

        public void Start()
        {
            character = new Character();
        }

        public IMenuContext Evaluate(string input, out string result)
        {
            result = "";
            if(creationStep == CharacterCreationStep.Start)
            {
                creationStep = CharacterCreationStep.Class;
                string value = "What do you want to be?\n";
                foreach (var c in Factory.GetAllClasses())
                    value += $"\t\t{c.ClassName} - {c.Description}\n";
                result += value;
                return this;
            }

            switch (creationStep)
            {
                case CharacterCreationStep.Class:
                    Class fetchedClass = Factory.GetClassByName(input);
                    if (fetchedClass != null)
                    {
                        string fullDetails = $"Class: {fetchedClass.ClassName}\n" +
                            $"{fetchedClass.GetFullDescription()}\n" +
                            "Base Stats:\n" +
                            $"\tMight: {fetchedClass.BaseStats.Might}\n" +
                            $"\tEndurance: {fetchedClass.BaseStats.Endurance}\n" +
                            $"\tLegerity: {fetchedClass.BaseStats.Legerity}\n" +
                            $"\tCunning: {fetchedClass.BaseStats.Cunning}\n" +
                            "Do you accept? (yes/no)";

                        result += fullDetails;
                        creationStep = CharacterCreationStep.ClassConfirm;
                        currentClassSelection = fetchedClass;
                        return this;
                    }
                    else
                    {
                        result += "Invalid class. Please choose again.\n";
                        return this;
                    }
                    break;

                case CharacterCreationStep.ClassConfirm:
                    if (input.StartsWith("y", true, null))
                    {
                        character.SetClass(currentClassSelection);
                        creationStep = CharacterCreationStep.Name;
                        result = $"What are you called, {currentClassSelection.ClassName}?";
                        return this;
                    }
                    else if (input.StartsWith("n", true, null))
                    {
                        creationStep = CharacterCreationStep.Class;
                        result += "What do you want to be?\n";
                        return this;
                    }
                    else
                    {
                        result = "Please answer yes or no.\n" +
                            "Do you accept? (yes/no)";
                        return this;
                    }
                    break;

                case CharacterCreationStep.Name:
                    string name = input.Trim();
                    if (name.Length < 1 || name.Length > 20)
                    {
                        result = "Name must be between 1 and 20 characters. Please choose again.";
                        return this;
                    }
                    else
                    {
                        result = $"So your name is {name}? (yes/no)";
                        character.Name = name;
                        creationStep = CharacterCreationStep.Confirm;
                        return this;
                    }
                    break;

                    case CharacterCreationStep.Confirm:
                    if(input.StartsWith("y", true, null))
                                            {
                        // Save character to file or database here
                        result = $"Character {character.Name} the {character.CharacterClass.ClassName} created successfully!\n";
                        if(!Directory.Exists(Paths.CharactersPath))
                            Directory.CreateDirectory(Paths.CharactersPath);
                        var savePath = Path.Combine(Paths.CharactersPath, $"{character.Name}.tres");
                        ResourceSaver.Save(character, savePath);
                        return null; // Exit context
                    }
                    else if (input.StartsWith("n", true, null))
                    {
                        creationStep = CharacterCreationStep.Name;
                        result = "What are you called?";
                        return this;
                    }
                    else
                    {
                        result = "Please answer yes or no.\n" +
                            $"So your name is {character.Name}? (yes/no)";
                        return this;
                    }
                    break;
            }

            result = "Invalid Character Creation stage. Not implemented yet.";
            return null;
        }
    }
}
