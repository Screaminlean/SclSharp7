//------------------------------------------------------------------------------
// If you are compiling for UWP verify that WINDOWS_UWP or NETFX_CORE are 
// defined into Project Properties->Build->Conditional compilation symbols
//------------------------------------------------------------------------------
#if WINDOWS_UWP || NETFX_CORE
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
#else // <-- Including MONO
#endif

namespace SclSharp7
{
    public partial class S7Client
	{
        public class CommentForces
		{
			// Only symbol table's with.seq extension
			public List<ForceJob> AddForceComments(string filepath, List<ForceJob> actualForces)
			{
				if (Path.GetExtension(filepath).ToLower() == ".seq")
				{
					var SymbolTableDataText = ReadSymbolTable(filepath);
					if (SymbolTableDataText.Length >= 1)
					{
						var SymbolTableDataList = ConvertDataArrToList(SymbolTableDataText);
						var CommentedForceList = AddCommentToForce(actualForces, SymbolTableDataList);
						return CommentedForceList;
					}
				}
				return ErrorSymbTableProces(actualForces);
			}

			private List<ForceJob> AddCommentToForce(List<ForceJob> forceringen, List<SymbolTableRecord> symbolTable)
			{
				List<ForceJob> commentedforces = new List<ForceJob>();

				foreach (ForceJob force in forceringen)
				{

					var found = symbolTable.Where(s => s.Address == force.FullAdress).FirstOrDefault();
					ForceJob commentedforce = new ForceJob();
					commentedforce = force;


					if (found != null)
					{
						commentedforce.Symbol = found.Symbol;
						commentedforce.Comment = found.Comment;
					}
					else
					{
						commentedforce.Symbol = "NOT SET";
						commentedforce.Comment = "not in variable table";
					}
					commentedforces.Add(commentedforce);

				}

				return commentedforces;

			}

			private List<SymbolTableRecord> ConvertDataArrToList(string[] text)
			{
				List<SymbolTableRecord> Symbollist = new List<SymbolTableRecord>();

				foreach (var line in text)
				{
					SymbolTableRecord temp = new SymbolTableRecord();
					string[] splited = new string[10];
					splited = line.Split('\t');
					temp.Address = splited[1];
					temp.Symbol = splited[2];
					temp.Comment = splited[3];

					Symbollist.Add(temp);
				}
				return Symbollist;
			}

			private string[] ReadSymbolTable(string Filepath)
			{

				string[] lines = System.IO.File.ReadAllLines(Filepath);
				return lines;
			}



			private List<ForceJob> ErrorSymbTableProces(List<ForceJob> actualForces)
			{
				var errorForceTable = actualForces;
				foreach (var forcerecord in errorForceTable)
				{
					forcerecord.Comment = "Force Table could not be processed";
					forcerecord.Symbol = "ERROR";

				}
				return errorForceTable;
			}
		}
	}
}
