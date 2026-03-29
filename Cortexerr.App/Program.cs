using Cortexerr.Core.Configuration;
using Cortexerr.Core.Utilities;
using Cortexerr.Core.Errors;
using Cortexerr.Core.Logging;
using Cortexerr.Core.Downloaders;
using System.Text.Json;
using Cortexerr.Core.Indexers;
using Cortexerr.Core.Arrs;
using Cortexerr.App.Base;
using Cortexerr.Extended.Indexer;
using Cortexerr.Extended.DataStructures;
using Cortexerr.Core.Ingest;
using Cortexerr.Decisions.Logic.Filter;

Arg.Initialize();
Logger.Initialize(); // logger dependant on args 
// everything past can use logger
Env.Initialize();
Config.Initialize();
// everything past must not throw errors

State.Initialize();
Routes.Initialize();
