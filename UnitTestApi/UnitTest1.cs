using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

using Newtonsoft.Json;

using ApiLayer;
using DataAccessLayer;
using Resources;


namespace UnitTestApi
{
	[TestClass]
	public class UnitTest1
	{
		DoctorList doctorList;
		TreatmentRoomList treatmentRoomList;
		TreatmentMachineList treatmentMachineList;
		PatientList patientList;
		ConsultationList consultationList;
		DateTime registrationDate = DateTime.Now.Date;



		[TestMethod]
		public void RegisterPatient()
		{
			// Set up mocked resources to return default content
			var mockCommands = new Moq.Mock<IDataAccess>();
			mockCommands.Setup(x => x.LoadConsultations()).Returns(consultationList);
			mockCommands.Setup(x => x.LoadDoctors()).Returns(doctorList);
			mockCommands.Setup(x => x.LoadPatients()).Returns(patientList);
			mockCommands.Setup(x => x.LoadTreatmentMachines()).Returns(treatmentMachineList);
			mockCommands.Setup(x => x.LoadTreatmentRooms()).Returns(treatmentRoomList);

			// Get the count for the Consultations loaded in TestInitialize
			var consultationCount = this.consultationList.Consultations.Count;

			// Use Constructor dependency injestion on the "Filter" classs to
			// pass the mocked IDataAccess object containing the lists of objects above.

			// Run the registration for the patient
			var commands = new Commands(mockCommands.Object);
			var consultationsJson = commands.RegisterPatient("Register|PatientUnitTest|Flu");

			// Get the updated set of Consultations.  Should include one new Consultation for "PatientUnitTest"
			var list = JsonConvert.DeserializeObject<ConsultationList>(consultationsJson);
			var c = list.Consultations[1];
			var newCount = list.Consultations.Count;

			DisplayConsultations(list.Consultations, "Here are the results:");

			var expected = consultationCount + 1;
			var actual = newCount;
			Assert.AreEqual(expected, actual, "Actual <> Expected");


			// TODO:  (Resolved ... see below)
			// Additional validation could be done here to examine each property of Consultation to ensure we 
			// have the expected date, doctor and room based on the preconditions.
			// This is just one example of the unit testing that could be done.
			//
			// One benefit of dynamically creating the JSON for Consultations is that the date information 
			// can be dynamic based on the current date.  
			// NOTE:  I did not get fully through debugging this test method.  I was getting a JSON error
			// while attempting to Deserialize the Patient.json file that was previously Serialized.
			// I need to spend more time with the Newtonsoft package to better understand it so that I can
			// work through issues more quickly.

			// RESOLVED:  This was resolved by constructing/serializing at the <Resource>List level for each
			// of the resources.
		}

		public void DisplayPatients(List<Patient> patients, string message)
		{
			Console.WriteLine(message);
			foreach (var patient in patients)
			{
				Console.WriteLine("Patient Name:  " + patient.Name);
			}
		}


		public void DisplayConsultations(List<Consultation> consultations, string message)
		{
			Console.WriteLine(message);
			foreach (var c in consultations)
			{
				Console.WriteLine("Patient Name:  " + c.Patient);
			}
		}


		/// <summary>
		/// NOTE:  For better designed unit testing I would attempt to bypass the creation of JSON files
		/// and instead just work with collections of objects.  
		/// One possible way is to introduce interfaces and mocking classes to the data access layer.
		/// I have a general understanding of mocking, but have no practical experience with it.
		/// In this case I would use a mock object to simulate the data access layer so that I could
		/// provide all of the data needs via objects instead of JSON files. 
		/// </summary>
		[TestInitialize]
		public void TestInitialize()
		{
			// Create Doctors
			doctorList = new DoctorList
			{
				Doctors = new List<Doctor>
				{
					new Doctor {Name = "John", Roles = new List<string> {"Oncologist"}},
					new Doctor {Name = "Anna", Roles = new List<string> {"GeneralPractition"}},
					new Doctor {Name = "Laura", Roles = new List<string> {"GeneralPractitioner", "Oncologist"}}
				}
			};

			// Create Treatment Machines
			treatmentMachineList = new TreatmentMachineList
			{
				TreatmentMachines = new List<TreatmentMachine>
				{
					new TreatmentMachine {Capability = "Advanced", Name = "MachineA"},
					new TreatmentMachine {Capability = "Advanced", Name = "MachineB"},
					new TreatmentMachine {Capability = "Simple", Name = "MachineC"},
				}
			};

			// Create Treatment Rooms
			treatmentRoomList = new TreatmentRoomList
			{
				TreatmentRooms = new List<TreatmentRoom>
				{
					new TreatmentRoom {Name = "RoomOne"},
					new TreatmentRoom {Name = "RoomTwo"},
					new TreatmentRoom {Name = "RoomThree", TreatmentMachine = "MachineA"},
					new TreatmentRoom {Name = "RoomFour", TreatmentMachine = "MachineB"},
					new TreatmentRoom {Name = "RoomFive", TreatmentMachine = "MachineC"}
				}
			};

			// Create Patients  
			// TODO:  Construct RegistrationDate based on offsets from current date to drive test scenario
			var today = DateTime.Now.ToString("yyyyMMdd");
			patientList = new PatientList
			{
				Patients = new List<Patient>
				{
					new Patient
					{
						Name = "Patient_1",
						RegistrationDate = today,
						Condition = new Condition {Diagnosis = "Flu", Topography = ""}
					},
					new Patient
					{
						Name = "Patient_2",
						RegistrationDate = today,
						Condition = new Condition {Diagnosis = "Cancer", Topography = "Breast"}
					}
				}
			};

			// Create Consultations
			consultationList = new ConsultationList
			{
				Consultations = new List<Consultation>
				{
					new Consultation
					{
						Patient = "Patient_1",
						PatientCondition = "Flu",
						PatientTopography = "",
						Doctor = "Anna",
						DoctorRole = "Oncologist",
						TreatmentRoom = "RoomOne",
						MachineName = "",
						MachineCapability = "",
						ConsultationDate = today
					},
					new Consultation
					{
						Patient = "Patient_2",
						PatientCondition = "Cancer",
						PatientTopography = "Breast",
						Doctor = "John",
						DoctorRole = "Oncologist",
						TreatmentRoom = "RoomFive",
						MachineName = "MachineC",
						MachineCapability = "Simple",
						ConsultationDate = today
					},
				}
			};
		}



		//[TestMethod]
		//public void TestMethod1()
		//{
		//	var expected = 11;
		//	var actual = 1;
		//	Assert.AreEqual(expected, actual, "Actual <> Expected");
		//}


	}
}
