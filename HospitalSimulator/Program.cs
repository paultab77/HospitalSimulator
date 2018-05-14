using System;
using System.Data.SqlClient;
using System.Text;

using ApiLayer;

using DataAccessLayer;

namespace HospitalSimulator
{
	class Program
	{
		/// <summary>
		/// This is the entry point to the application.
		/// 
		/// As you can see
		/// </summary>
		/// <param name="args"></param>
		private static void Main(string[] args)
		{
			if (args.Length >= 1)
			{
				if (!args[0].ToString().Contains("?"))
				{
					var da = new DataAccess();
					var commands = new Commands(da);
					Console.Write(commands.ProcessRequest(args[0]));
					return;
				}
			}
			var apiHelp = new StringBuilder();
			apiHelp.AppendLine("Expected requests are:");
			apiHelp.AppendLine("1) Patient Registration.  \r\nExample:  HospitalSimulator.exe Register|PatientName|Condition|Topography");
			apiHelp.AppendLine("\r\n2) Get the list of registered patients.  \r\nExample:  HospitalSimulator.exe RegisteredPatients");
			apiHelp.AppendLine("\r\n3) Get the list of scheduled consultations.  \r\nExample:  HospitalSimulator.exe ScheduledConsultations");
			Console.Write(apiHelp.ToString());
		}
	}
}
