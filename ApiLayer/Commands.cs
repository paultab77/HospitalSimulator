/*	Purpose:  Provide the Command Line interface (API) for the Hospital Simulator.
 * 
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Newtonsoft.Json;

using DataAccessLayer;
using Resources;

namespace ApiLayer
{
	public interface ICommands
	{
		string RegisterPatient(string request);
	}


	public class Commands: ICommands
	{
		IDataAccess ida;

		public Commands(IDataAccess da)
		{
			ida = da;
		}

		/// <summary>
		/// Process the Request to the ApiLayer.
		/// Expected requests are:
		/// 1) Patient Registration.  Example:  Register|PatientName|Condition|Topography
		/// 2) Get the list of registered patients.  Example:  RegisteredPatients
		/// 3) Get the list of scheduled consultations.  Example:  ScheduledConsultations
		/// </summary>
		/// <param name="request"></param>
		/// <returns>A string containing the result of the request</returns>
		public string ProcessRequest(string request)
		{
			// Validate the request
			var cmdArray = request.Split('|');
			switch (cmdArray[0].ToUpper())
			{
				case "REGISTER":
					return RegisterPatient(request);
				case "REGISTEREDPATIENTS":
					return QuergyPatientRegistration();
				case "SCHEDULEDCONSULTATIONS":
					return QueryConsultation();
			}

			var apiHelp = new StringBuilder();
			apiHelp.AppendLine("Expected requests are:");
			apiHelp.AppendLine("1) Patient Registration.  \r\nExample:  HospitalSimulator.exe Register|PatientName|Condition|Topography");
			apiHelp.AppendLine("\r\n2) Get the list of registered patients.  \r\nExample:  HospitalSimulator.exe RegisteredPatients");
			apiHelp.AppendLine("\r\n3) Get the list of scheduled consultations.  \r\nExample:  HospitalSimulator.exe ScheduledConsultations");
			Console.Write(apiHelp.ToString());
			return apiHelp.ToString();
		}

		/// <summary>
		/// Validate the user request and pull out each argument from the request string.
		/// </summary>
		/// <param name="request">Original user request</param>
		/// <param name="name">Patient Name</param>
		/// <param name="condition">Patient Condition</param>
		/// <param name="topography">Patient Topography</param>
		/// <returns>Error string or empty if no error</returns>
		public static string ValidateRequest(string request, out string name, out string condition, out string topography)
		{
			name = string.Empty;
			condition = string.Empty;
			topography = string.Empty;

			// Validate the request
			// We are expecting the following formats:
			// 1) Register|PatientName|Condition|Topography
			// 2) Register|PatientName|Condition
			var cmdArray = request.Split('|');

			if ((cmdArray.Length < 3) || (cmdArray.Length > 4))
				return "Register command is invalid.  Expected format = 'Register|PatientName|Condition|Topography' or 'Register|PatientName|Condition'";

			// Validate Patient Name (Min characters = 1)
			name = cmdArray[1].Trim();
			if (name.Length == 0)
				return "Register command is invalid.  Name is required.  Expected format = 'Register|PatientName|Condition|Topography' or 'Register|PatientName|Condition'";

			// Validate Condition (Valid values are Flu and Cancer)
			condition = cmdArray[2].Trim().ToUpper();
			switch (condition)
			{
				case "FLU":
					if (cmdArray.Length == 4)
						return "Register command is invalid.  When Condition = 'Flu', Topography is not allowed.  Expected format = 'Register|PatientName|Condition|Topography' or 'Register|PatientName|Condition'";
					break;
				case "CANCER":
					if (cmdArray.Length == 3)
						return "Register command is invalid.  When Condition = 'Flu', Topography is not allowed.  Expected format = 'Register|PatientName|Condition|Topography' or 'Register|PatientName|Condition'";

					// Validate Topography
					topography = cmdArray[3].Trim().ToUpper();
					if ((topography != "HEAD&NECK") && (topography != "BREAST"))
						return "Register command is invalid.  Topography is invalid.  It must be either 'Head&Neck' or 'Breast'";
					break;
				default:
					return "Register command is invalid.  Condition is invalid.  It must be either 'Flu' or 'Cancer'.";
			}
			return string.Empty;
		}

		/// <summary>
		/// RegsiterPatient acts as the API for registering a patient.
		/// 
		/// The request is first validated.  If not valid an Error will be returned.
		/// If valid, a the Patient object will return in JSON.
		/// </summary>
		/// <param name="request"></param>
		/// <returns>String.  Results of the request.  
		/// On Success it returns a Patient object in JSON.  
		/// On Error, an Error string is returned</returns>
		public string RegisterPatient(string request)
		{
			// Note:  All of the APIs require exception handling.  The types of exceptions need to be identified and
			// the response returned to the caller needs to be clearly defined.  
			var name = "";
			var condition = "";
			var topography = "";
			var error = ValidateRequest(request, out name, out condition, out topography);
			if (error != string.Empty)
				return error;

			// If we got here, the arguments are valid.
			// Now it is time to Register the patient and add a Consult

			// Patient Registration (TODO:  Move to new method)
			// Normally there'd be a Medical Record Number or some other criteria to prevent from registering patients more than once.
			// For this exercise, no checking will be done for duplicate patients.
			var registrationDate = DateTime.Now;
			var patientRegistrations = QuergyPatientRegistrationAsObjects();
			var patient = new Patient { Name = name, Condition = new Condition { Diagnosis = condition, Topography = topography }, RegistrationDate = registrationDate.ToString("yyyyMMdd") };
			patientRegistrations.Patients.Add(patient); // This is where persistence would be performed.

			//*****************************************************************************************************************************
			// Gather resource information to be used for identifying the next available consultation slot
			//*****************************************************************************************************************************

			// Load the Doctors and create separate lists for General Practitioners and Oncologists
			//var ida = new DataAccess();
			var doctors = ida.LoadDoctors();
			var generalPractitioners = from d in doctors.Doctors
																 where (d.Roles.Contains("GeneralPractitioner"))
																 select d;
			var oncologists = from d in doctors.Doctors
												where (d.Roles.Contains("Oncologist"))
												select d;

			// Load the Treatment Machines and create separate lists for Advanced and Simple
			var treatmentMachines = ida.LoadTreatmentMachines();
			var tma = new List<string>();
			var tms = new List<string>();
			foreach (var t in treatmentMachines.TreatmentMachines)
			{
				if (t.Capability.ToUpper() == "ADVANCED")
					tma.Add(t.Name);
				else if (t.Capability.ToUpper() == "SIMPLE")
					tms.Add(t.Name);
			}

			// Load the Treatment Rooms and create separate lists for rooms based on machine capability
			var treatmentRooms = ida.LoadTreatmentRooms();
			var trNoMachine = new List<string>();
			var trMachineAdvanced = new List<string>();
			var trMachineSimple = new List<string>();
			foreach (var t in treatmentRooms.TreatmentRooms)
			{
				if (tma.Contains(t.TreatmentMachine))
					trMachineAdvanced.Add(t.Name);
				else if (tms.Contains(t.TreatmentMachine))
					trMachineSimple.Add(t.Name);
				else
					trNoMachine.Add(t.Name);
			}

			// Create a dictionary of Treatment Rooms
			var treatmentRoomDictionary = treatmentRooms.TreatmentRooms.ToDictionary(x => x.Name, x => x.TreatmentMachine);


			ConsultationList scheduledConsultations = null;
			IOrderedEnumerable<Consultation> sortedConsults = null;

			// Get the current set of scheduled consultations
			try
			{
				scheduledConsultations = ida.LoadConsultations();
				sortedConsults = scheduledConsultations.Consultations.OrderBy(c => c.ConsultationDate);
			}
			catch (Exception exception)
			{
				// TODO:  Determine what type of error handling is required.
				// There is a difference betwen no Consultations and an Error attempting to retrieve consultations.
				// For this exercise I assume that there are consultations and the only reason we got here is because there was a problem accessing the data.
				// An appropriate error should be returned here.  The error could say something like "API data access issues encountered.  Please try again later".
				// ReSharper disable once PossibleIntendedRethrow
				throw (exception);
			}

			//*****************************************************************************************************************************
			// Use Case 1:  Patient.Condition.Diagnosis = Flu
			//
			// Find the next slot where:
			//		Doctor.Role = GeneralPractitioner
			//		TreatmentRoom (No filtering required)
			//
			// To accomplish this we will step through the ordered collection looking for an open slot.
			//
			// Starting at Registration Date + 1 look for the first open slot for any GP / Treatment Room
			//*****************************************************************************************************************************
			if (condition == "FLU")
			{
				// The patient consult must not be on the Registration Date
				var consultDate = registrationDate.Date.AddDays(1);

				var consultAdded = false;
				while (!consultAdded)
				{
					var openRoomName = string.Empty;

					// Get the active consults for the proposed consult date
					var date = consultDate;
					var activeConsults =
							sortedConsults.OrderBy(c => c.ConsultationDate).Where(c => c.ConsultationDateAsDateTime() == date);

					// If no consults
					var consultations = activeConsults as Consultation[] ?? activeConsults.ToArray();
					if (!consultations.Any())
					{
						// Add new Consult
						var consult = new Consultation
						{
							Patient = patient.Name,
							PatientCondition = "Flu",
							Doctor = generalPractitioners.ElementAt(0).Name,
							DoctorRole = "GeneralPractitioner",
							TreatmentRoom = trNoMachine[0],
							MachineName = treatmentRoomDictionary[trNoMachine[0]],
							MachineCapability = "TODO",
						ConsultationDate = consultDate.ToString("yyyyMMdd"),
						};

						// Add the Consult
						scheduledConsultations.Consultations.Add(consult);
						consultAdded = true;
						break;
					}

					// Get the Treatment Room Names for the active consults
					var consultRooms = consultations.Select(pdc => pdc.TreatmentRoom).ToList();

					// Compare the total list of treatment rooms to the active consult rooms to see if there are any empty rooms
					foreach (var t in treatmentRooms.TreatmentRooms)
					{
						// If there is an empty room
						if (!consultRooms.Contains(t.Name))
						{
							// Save the room name 
							openRoomName = t.Name;
							break;
						}
					}

					if (openRoomName != string.Empty)
					{
						// Get the list of General Practitioner Names for the active consults
						var consultDoctors = consultations.Where(pdc => pdc.DoctorRole == "GeneralPractitioner").Select(pdc => pdc.Doctor).ToList();

						// Search for an unscheduled GP
						foreach (var t in generalPractitioners)
						{
							// If there is an unscheduled GP
							if (!consultDoctors.Contains(t.Name))
							{
								// Add new Consult
								var consult = new Consultation
								{
									Patient = patient.Name,
									PatientCondition = "Flu",
									Doctor = t.Name,
									DoctorRole = "GeneralPractitioner",
									TreatmentRoom = openRoomName,
									MachineName = treatmentRoomDictionary[openRoomName],
									MachineCapability = "TODO",
								ConsultationDate = consultDate.ToString("yyyyMMdd"),
								};

								// Add the Consult
								scheduledConsultations.Consultations.Add(consult);
								consultAdded = true;
								break;
							}
						}
					}

					// No open slot found.  Increment Consult Date and try again.
					consultDate = consultDate.AddDays(1);
				}
			}

			else if (condition == "CANCER")
			{
				//*****************************************************************************************************************************
				// Use Case 2:  
				//		Patient.Condition.Diagnosis = Cancer
				//		Patient.Condition.Topography = Breast or Head&Neck
				//		Doctor.Role = Oncologist
				//		TreatmentRoom.Treatment.Machine => TreatmentMachine.Capability = Simple or Advanced (for Breast) or
				//		TreatmentRoom.Treatment.Machine => TreatmentMachine.Capability = Advanced (for Head&Neck)
				//
				// To accomplish this we will step through the ordered collection looking for an open slot.
				//
				// Starting at Registration Date + 1 look for the first open slot for any Oncologist / Treatment Room (with Machine 
				// Capability = Advanced or Simple).
				//
				// TODO:  Look at combining code with Flu.
				//*****************************************************************************************************************************

				// The patient consult must not be on the Registration Date
				var consultDate = registrationDate.Date.AddDays(1);

				var consultAdded = false;
				while (!consultAdded)
				{
					var openRoomName = string.Empty;

					// Get the active consults for the proposed consult date
					var activeConsults =
							sortedConsults.OrderBy(c => c.ConsultationDate).Where(c => c.ConsultationDateAsDateTime() == consultDate);

					// If no consults
					if (!activeConsults.Any())
					{
						// Add new Consult
						var consult = new Consultation
						{
							Patient = patient.Name,
							PatientCondition = "Cancer",
							PatientTopography = topography,
							Doctor = oncologists.ElementAt(0).Name,
							DoctorRole = "Oncologist",
							TreatmentRoom = trNoMachine[0],
							MachineName = treatmentRoomDictionary[trNoMachine[0]],
							MachineCapability = "TODO",
						ConsultationDate = consultDate.ToString("yyyyMMdd"),
						};

						scheduledConsultations.Consultations.Add(consult);
						consultAdded = true;
						break;
					}

					// Get the Treatment Room Names for the active consults
					var consultRooms = activeConsults.Select(pdc => pdc.TreatmentRoom).ToList();

					// For topography = Breast search for open consult rooms where Machine Capability = Simple
					if (topography == "BREAST")
					{
						// Compare the total list of rooms to the active consult rooms to see if there are any empty rooms
						foreach (var t in trMachineSimple)
						{
							// If there is an empty room
							if (!consultRooms.Contains(t))
							{
								// Save the room name 
								openRoomName = t;
								break;
							}
						}
					}

					// If no open treatment rooms (Machine Capability = Simple)
					if (openRoomName == string.Empty)
					{
						// Compare the total list of treatment rooms (Machine = Advanced) to the active consult rooms to see if there are any empty rooms
						foreach (var t in trMachineAdvanced)
						{
							// If there is an empty room
							if (!consultRooms.Contains(t))
							{
								// Save the room name 
								openRoomName = t;
							}
						}
					}

					if (openRoomName != string.Empty)
					{
						// Get the list of Oncologist Names for the active consults
						var consultDoctors = activeConsults.Where(pdc => pdc.DoctorRole == "Oncologist").Select(pdc => pdc.Doctor).ToList();

						// Search for an unscheduled Oncologist
						foreach (var t in oncologists)
						{
							// If there is an empty room
							if (!consultDoctors.Contains(t.Name))
							{
								// Add new Consult
								var consult = new Consultation
								{
									Patient = patient.Name,
									PatientCondition = "Cancer",
									PatientTopography = topography,
									Doctor = t.Name,
									DoctorRole = "Oncologist",
									TreatmentRoom = openRoomName,
									MachineName = treatmentRoomDictionary[openRoomName],
									MachineCapability = "TODO",
									ConsultationDate = consultDate.ToString("yyyyMMdd"),
								};

								scheduledConsultations.Consultations.Add(consult);
								consultAdded = true;
								break;
							}
						}
					}

					// No open slot found.  Increment Consult Date and try again.
					consultDate = consultDate.AddDays(1);
				}
			}

			var consults = JsonConvert.SerializeObject(scheduledConsultations);
			return consults;
		}


		/// <summary>
		/// QuergyPatientRegistration is used for querying patient registrations.
		/// 
		/// No parameters are provided, so all patient registrations will be retrieved.
		/// </summary>
		/// <returns>String.  The list of Patient Registraton objects in JSON.</returns>
		public PatientList QuergyPatientRegistrationAsObjects()
		{
			return ida.LoadPatients();
		}


		/// <summary>
		/// QuergyPatientRegistration acts as the API for querying patient registrations.
		/// 
		/// No parameters are provided, so all patient registrations will be retrieved.
		/// </summary>
		/// <returns>String.  The list of Patient Registraton objects in JSON.</returns>
		public string QuergyPatientRegistration()
		{
			var patientList = ida.LoadPatients();
			var patientListJson = JsonConvert.SerializeObject(patientList);

			return patientListJson;
		}


		/// <summary>
		/// QueryConsultation is used for querying scheduled consultations.
		/// 
		/// No parameters are provided, so all scheduled consultations will be retrieved.
		/// </summary>
		/// <returns>String.  The list of Consultation objects in JSON.</returns>
		public ConsultationList QueryConsultationAsObject()
		{
			return ida.LoadConsultations();
		}


		/// <summary>
		/// QueryConsultation acts as the API for querying patient registrations.
		/// 
		/// No parameters are provided, so all patient registrations will be retrieved.
		/// </summary>
		/// <returns>String.  The list of Consultation objects in JSON.</returns>
		public string QueryConsultation()
		{
			var consultationList = ida.LoadConsultations();
			var consultationListJson= JsonConvert.SerializeObject(consultationList);
			return consultationListJson;
		}
		
		
	}
}
