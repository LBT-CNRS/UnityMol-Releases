/*
    ================================================================================
    Copyright Centre National de la Recherche Scientifique (CNRS)
        Contributors and copyright holders :

        Xavier Martinez, 2017-2022
        Hubert Santuz, 2022-2026
        Marc Baaden, 2010-2026
        unitymol@gmail.com
        https://unity.mol3d.tech/

        This file is part of UnityMol, a general framework whose purpose is to provide
        a prototype for developing molecular graphics and scientific
        visualisation applications based on the Unity3D game engine.
        More details about UnityMol are provided at the following URL: https://unity.mol3d.tech/

        This program is free software: you can redistribute it and/or modify
        it under the terms of the GNU General Public License as published by
        the Free Software Foundation, either version 3 of the License, or
        (at your option) any later version.

        This program is distributed in the hope that it will be useful,
        but WITHOUT ANY WARRANTY; without even the implied warranty of
        MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
        GNU General Public License for more details.

        You should have received a copy of the GNU General Public License
        along with this program. If not, see <https://www.gnu.org/licenses/>.

        To help us with UnityMol development, we ask that you cite
        the research papers listed at https://unity.mol3d.tech/cite-us/.
    ================================================================================
*/
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UMol.API;

namespace UMol {

/// <summary>
/// Simpler Interface to the API when the python interface is not available
/// <remarks>Used in Multiplayer mode for Android builds to receive and execute API commands.</remarks>
/// </summary>
public class InterfaceAPIPython : MonoBehaviour
{
    /// <summary>
    /// Mapping between the list of API function infos (in the `APIPython` class) and their names cast in string
    /// </summary>
    private static readonly Dictionary<string, List<MethodInfo>> commandList = getCommandMap<APIPython>();

    /// <summary>
    /// Construct a dictionary where each key is a function name and each value a list of the function info
    /// </summary>
    /// <typeparam name="T">Generic class to parse the functions</typeparam>
    /// <returns>the dictionary created</returns>
    private static Dictionary<string, List<MethodInfo>> getCommandMap<T>()
    {
        Dictionary<string, List<MethodInfo>> commandMap = new();
        Type type = typeof(T);
        MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);

        foreach (MethodInfo method in methods)
        {
            if (!commandMap.ContainsKey(method.Name))
            {
                commandMap[method.Name] = new List<MethodInfo>(); // for handling overloaded methods
            }
            commandMap[method.Name].Add(method);
        }

        return commandMap;
    }

    /// <summary>
    /// Execute an API command based on the literal name.
    /// Assumes the commands contain all the necessary parameters to properly parse them.
    /// </summary>
    /// <param name="commandName">name of the command to execute</param>
    /// <returns>an object containing the return value of the command.</returns>
    public static object ExecuteCommand(string commandName)
    {
        Debug.Log("inputCommand: " + commandName);
        List<string> parsedCommand = parseCommand(commandName);
        Debug.Log("count " + parsedCommand.Count);

        List<MethodInfo> methods; // a list to handle overloaded methods
        if (!commandList.TryGetValue(parsedCommand[0], out methods))
        {
            Debug.LogError($"Command {parsedCommand[0]} not found!");
            return null;
        }

        // Find the best method match based on argument count
        string[] args = parsedCommand.GetRange(1, parsedCommand.Count - 1).ToArray();
        MethodInfo methodToInvoke = methods.Find(m => m.GetParameters().Length == args.Length);

        // the method.Invoke require args with specific type
        ParameterInfo[] parameters = methodToInvoke.GetParameters();
        object[] convertedArgs = convertArgs(args, parameters);

        Debug.Log($"Method '{parsedCommand[0]}' expects {parameters.Length} parameters.");

        foreach (ParameterInfo param in parameters)
        {
            Debug.Log($"Parameter: {param.Name}, Type: {param.ParameterType}");
        }

        return methodToInvoke.Invoke(null, convertedArgs);

    }

    /// <summary>
    /// We parse the commands with the corresponding parameters.
    /// We assume the input string contains all the parameters since
    /// it is transmitted as a RPC from the pythonRecord which explicitly sets that.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    private static List<string> parseCommand(string input)
    {
        // Match commandName(args)
        Match match = Regex.Match(input, @"^(\w+)\((.*)\)$");

        if (!match.Success)
        {
            Console.WriteLine("Invalid command format!");
            return null;
        }

        string commandName = match.Groups[1].Value;
        Debug.Log("commandName " + commandName);
        string argsString = match.Groups[2].Value.Trim();

        List<string> result = new() {
            commandName
        };

        if (string.IsNullOrEmpty(argsString)) {
            return result;
        }

        // Split arguments by commas, preserving values inside quotes
        MatchCollection argMatches = Regex.Matches(argsString, @"(\w+\s*=\s*[^,]+|[^,]+)");

        foreach (Match argMatch in argMatches)
        {
            string arg = argMatch.Value.Trim('"');

            if (arg.Contains("="))
            {
                // Named argument: key=value
                string[] parts = arg.Split('=');
                string key = parts[0].Trim();
                string value = parts[1].Trim('"');

                result.Add(value);
                Debug.Log("arg " + value);
            }
            else
            {
                // Positional argument
                result.Add(arg);
                Debug.Log("arg " + arg);
            }
        }
        return result;
    }

    /// <summary>
    /// Since the Invoke call requires the exact types for the parameters,
    /// we assume the command has all the parameters in the correct order and convert them into
    /// the appropriate type.
    /// </summary>
    /// <param name="args"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    private static object[] convertArgs(object[] args, ParameterInfo[] parameters)
    {
        object[] converted = new object[parameters.Length];

        for (int i = 0; i < parameters.Length; i++)
        {
            Type paramType = parameters[i].ParameterType;
            if (i < args.Length)
            {
                converted[i] = Convert.ChangeType(args[i], paramType);
            }
            else
            {
                converted[i] = parameters[i].HasDefaultValue ? parameters[i].DefaultValue : Activator.CreateInstance(paramType);
            }
        }
        return converted;
    }
}
}
