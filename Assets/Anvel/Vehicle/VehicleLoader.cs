using UnityEngine;

using System;
using System.IO;
using System.Collections.Generic;

namespace CAVS.Anvel.Vehicle
{
    public static class VehicleLoader
    {
        public static TimeEntry[] LoadVehicleData(string filePath)
        {

            bool eatingHeader = true;

            // Header
            int numberOfVehicleObjects = 0;

            // Body
            int currentVehicleObjectIndex = 0;
            List<TimeEntry> timeEntries = new List<TimeEntry>();
            TimeEntry currentTimeEntry = null;

            foreach (var line in File.ReadAllLines(filePath))
            {
                if (eatingHeader)
                {
                    if (line.Contains("Vehicle Objects"))
                    {
                        numberOfVehicleObjects = int.Parse(line.Substring(16));
                    }
                    else if (line == "@Data")
                    {
                        eatingHeader = false;
                    }
                }
                else
                {
                    if (currentVehicleObjectIndex == 0)
                    {
						if(currentTimeEntry != null) {
							timeEntries.Add(currentTimeEntry);
						}

                        var contents = line.Split(':');
                        currentTimeEntry = new TimeEntry()
                        {
                            Time = float.Parse(contents[0]),
                            Transforms = new List<TransformEntry>() {
                                ParseEntry(new String[] {
                                    contents[1],
                                    contents[2]
                                })
                            }
                        };
                    } else {
						currentTimeEntry.Transforms.Add(ParseEntry(line.Split(':')));
					}

                    currentVehicleObjectIndex++;
                    if (currentVehicleObjectIndex > numberOfVehicleObjects)
                    {
                        currentVehicleObjectIndex = 0;
                    }
                }

            }

			return timeEntries.ToArray();
        }

        private static TransformEntry ParseEntry(string[] entry)
        {
            string[] positionEntrys = entry[0].Split(' ');
            string[] rotationEntrys = entry[1].Split(' ');
            return new TransformEntry()
            {
                Position = new Vector3(
                    float.Parse(positionEntrys[1]),
                    float.Parse(positionEntrys[2]),
                    float.Parse(positionEntrys[0])
                ),
                Rotation = new Quaternion(
                    float.Parse(rotationEntrys[1]),
                    float.Parse(rotationEntrys[2]),
                    float.Parse(rotationEntrys[0]),
                    float.Parse(rotationEntrys[3])
                )
            };
        }

    }

}