﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System;
using System.Net.Http;
using System.Linq;
using Unity.Plastic.Newtonsoft.Json;

public class ColectData : MonoBehaviour
{
    // Model
    public class KartModel
    {
        public int id { get; set; }
        public int fileId { get; set; }
        public double time { get; set; }
        public double xPos { get; set; }
        public double yPos { get; set; }
        public double zPos { get; set; }
        public bool leftSide { get; set; }
        public bool leftForward { get; set; }
        public bool centralForward { get; set; }
        public bool rightForward { get; set; }
        public bool rightSide { get; set; }
        public double leftSideDistance { get; set; }
        public double leftForwardDistance { get; set; }
        public double centralForwardDistance { get; set; }
        public double rightForwardDistance { get; set; }
        public double rigthSideDistance { get; set; }
        public string zone { get; set; }
        public bool movingForward { get; set; }
    }

    //Global variables
    // sensors - raycast hit
    [Header("Sensor")]
    public float sensorLength = 5f;
    public Vector3 frontSensorPosition = new Vector3(0f, 0.2f, 0.2f);
    public float frontSideSensorPosition = 0.5f;
    public float frontSensorAngle = 30;
    public bool avoiding = false;
    List<bool> wallDetected = new List<bool> { false, false, false, false, false };
    List<double> distanceToWall = new List<double> { 5f, 5f, 5f, 5f, 5f };
    //Export
    private string fileName = "export_" + DateTime.Now.ToString("M_dd_HH_mm") + ".csv";
    private string fileId = DateTime.Now.ToString("Mddhhmm");
    //Moving Forward or BackWard
    public Vector3 LastPosition;
    public Vector3 ActualPosition;
    public bool MovingForward = false;
    public bool MovingBackward = false;
    string zone = "";
    string currZone;
    float x;
    float y;
    float z;


    // Start is called before the first frame update
    private void Start()
    {
        try
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:/Poli/Dizertatie/exports/"+fileName, true))
            {
                file.WriteLine("fileId,time,xPos,yPos,zPos,leftSide,leftForward,centralForward,rightForward,rightSide,leftSideDistance,leftForwardDistance," +
                    "centralForwardDistance,rightForwardDistance,rightSideDistance,zone,movingForward");
            }
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Error ", ex);
        }
    }


    // Update is called once per frame
    void Update()
    {

        //Store data in CSV
        var csv = new StringBuilder();
        x = transform.position.x;
        y = transform.position.y;
        z = transform.position.z;
        Sensors();
        currZone=Moving();
        // create the rows you need to append
        StringBuilder sb = new StringBuilder();
        // time passed, x-poz, y-poz, z-poz, left angle sensor, left forward sensor, central forward sensor, rigth forward sensor, right angle sensor, 
        // left angle sensor distance, left forward sensor distance, central forward sensor distance, rigth forward sensor distance, right angle sensor distance, 
        //zone, moving forward
        sb.AppendFormat("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16}\n",fileId,Time.time.ToString(), x.ToString(),
            y.ToString(), z.ToString(),
            wallDetected[0].ToString(),wallDetected[1].ToString(), wallDetected[2].ToString(),wallDetected[3].ToString(), wallDetected[4].ToString(),
            distanceToWall[0].ToString(), distanceToWall[1].ToString(), distanceToWall[2].ToString(), distanceToWall[3].ToString(), distanceToWall[4].ToString(),
            currZone,MovingForward, Environment.NewLine);

        // flush all rows once time.
        //File.AppendAllText(@"C:/Poli/Dizertatie/exports/" + file_name, sb.ToString(), Encoding.UTF8);

       // PostAPI();
        print("fileId " + fileId 
            +" xPos" + x.ToString() 
            + "yPos:" + y.ToString() 
            + "zPos: " + z.ToString() 
            + "leftSide: " + wallDetected[0] 
            + "leftForward: " + wallDetected[1]
            + "centralForward: " + wallDetected[2]
            + "rightForward: " + wallDetected[3]
            + "rightSide: " + wallDetected[4]
            + "leftSideDistance: " + wallDetected[0]
            + "leftForwardDistance: " + wallDetected[1]
            + "centralForwardDistance: " + wallDetected[2]
            + "rightForwardDistance: " + wallDetected[3]
            + "rightSideDistance: " + wallDetected[4]
            + "zone: "+ currZone
            + "movingForward "+ MovingForward);

    }

    // Read data from sensors
    private void Sensors()
    {
        RaycastHit hit;
        Vector3 sensorStartingPos = transform.position;
        sensorStartingPos += transform.forward * frontSensorPosition.z;
        sensorStartingPos += transform.up * frontSensorPosition.y;
        avoiding = false;
        wallDetected[0] = false; //Left Angle Side
        wallDetected[1] = false; //Left Front Side
        wallDetected[2] = false; //Front Center Side
        wallDetected[3] = false; //Right Front Side
        wallDetected[4] = false; //Right Angle Side 
        distanceToWall[0] = 5f; //Left Angle Side
        distanceToWall[1] = 5f; //Left Front Side
        distanceToWall[2] = 5f; //Front Center Side
        distanceToWall[3] = 5f; //Right Front Side
        distanceToWall[4] = 5f; //Right Angle Side 
        bool wallRaycastHit = false;
        // front center sensor
        if (Physics.Raycast(sensorStartingPos, transform.forward, out hit, sensorLength))
        {
            if (hit.collider.CompareTag("Track"))
            {
                Debug.DrawLine(sensorStartingPos, hit.point, Color.red);
                wallDetected[2] = true;
                avoiding = true;
                print("Hit something in forward at" + hit.distance +"meters");
                distanceToWall[2] = hit.distance;
                wallRaycastHit = true;
            }
        }


        // front right sensor 
        sensorStartingPos += transform.right * frontSideSensorPosition;
        if (Physics.Raycast(sensorStartingPos, transform.forward, out hit, sensorLength))
        {
            if (hit.collider.CompareTag("Track"))
            {
                Debug.DrawLine(sensorStartingPos, hit.point, Color.yellow);
                wallDetected[3] = true;
                avoiding= true;
                print("Hit something in rigth forward at" + hit.distance + "meters");
                distanceToWall[3] = hit.distance;
                wallRaycastHit = true;
            }
        }


        // front right angle sensor 
        if (Physics.Raycast(sensorStartingPos, Quaternion.AngleAxis(frontSensorAngle, transform.up) * transform.forward, out hit, sensorLength))
        {
            if (hit.collider.CompareTag("Track"))
            {
                Debug.DrawLine(sensorStartingPos, hit.point, Color.green);
                wallDetected[4] = true;
                avoiding = true;
                print("Hit something in rigth side at" + hit.distance + "meters");
                distanceToWall[4] = hit.distance;
                wallRaycastHit = true;
            }
        }


        // front left sensor
        sensorStartingPos -= transform.right * frontSideSensorPosition * 2;
        if (Physics.Raycast(sensorStartingPos, transform.forward, out hit, sensorLength))
        {
            if (hit.collider.CompareTag("Track"))
            {
                Debug.DrawLine(sensorStartingPos, hit.point, Color.yellow);
                wallDetected[1] = true;
                avoiding = true;
                print("Hit something in left forward at" + hit.distance + "meters");
                distanceToWall[1] = hit.distance;
                wallRaycastHit = true;
            }
        }


        // front left angle sensor 
        if (Physics.Raycast(sensorStartingPos, Quaternion.AngleAxis(-frontSensorAngle, transform.up) * transform.forward, out hit, sensorLength))
        {
            if (hit.collider.CompareTag("Track"))
            {
                Debug.DrawLine(sensorStartingPos, hit.point, Color.green);
                wallDetected[0] = true;
                avoiding = true;
                print("Hit something in left side at" + hit.distance + "meters");
                distanceToWall[0] = hit.distance;
                wallRaycastHit = true;
            }
        }

        if (avoiding)
        {
            print("\nWall Sensor:" + "Left Angle Side:" + wallDetected[0] + "\nLeft Front Side:" + wallDetected[1] + "\nFront Side" 
                + wallDetected[2] + "\nRight Front Side:" + wallDetected[3] + "\nRight Angle Side:" + wallDetected[4]);
            if(wallRaycastHit)
                print("\nWall Sensor Distance:" + "Left Side Distance:" + distanceToWall[0] + "\nLeft Front Distance:" + distanceToWall[1] + "\nFront Central Distance"
                + distanceToWall[2] + "\nRight Front Distance:" + distanceToWall[3] + "\nRight Side Distance:" + distanceToWall[4]);

        }
    }

    // check in which zone the kart is and if the kart is moving forward
    public string Moving()
    {
        zone = "";
        ActualPosition.x = transform.position.x;
        ActualPosition.z = transform.position.z;

        MovingBackward = false;
        MovingForward = false;

        //Zone 1
        if((ActualPosition.x < 22f) && (ActualPosition.x > 8) && (ActualPosition.z > -41f) && (ActualPosition.z <60f))
        {
            zone = "Zone 1";

            if (LastPosition.z < ActualPosition.z)
            {
                MovingForward = true;
                MovingBackward = false;
            }

            if (LastPosition.z > ActualPosition.z)
            {
                MovingForward = false;
                MovingBackward = true;
            }
            else if (LastPosition.z == ActualPosition.z)
            {
                MovingForward = false;
                MovingBackward = false;
            }
        }

        //Zone 2
        if ((ActualPosition.x < 10f) && (ActualPosition.x > -40) && (ActualPosition.z > 45f) && (ActualPosition.z < 62f))
        {
            zone = "Zone 2";

            if (LastPosition.x > ActualPosition.x)
            {
                MovingForward = true;
                MovingBackward = false;
            }

            if (LastPosition.x < ActualPosition.x)
            {
                MovingForward = false;
                MovingBackward = true;
            }
            else if (LastPosition.x == ActualPosition.x)
            {
                MovingForward = false;
                MovingBackward = false;
            }
        }

        //Zone 3
        if ((ActualPosition.x < -35f) && (ActualPosition.x > -50) && (ActualPosition.z > -45f) && (ActualPosition.z < 50f))
        {
            zone = "Zone 3";

            if (LastPosition.z > ActualPosition.z)
            {
                MovingForward = true;
                MovingBackward = false;
            }

            if (LastPosition.z < ActualPosition.z)
            {
                MovingForward = false;
                MovingBackward = true;
            }
            else if (LastPosition.z == ActualPosition.z)
            {
                MovingForward = false;
                MovingBackward = false;
            }
        }

        //Zone 4
        if ((ActualPosition.x < 20f) && (ActualPosition.x > -35f) && (ActualPosition.z > -50f) && (ActualPosition.z < -30f))
        {
            zone = "Zone 4";

            if (LastPosition.x < ActualPosition.x)
            {
                MovingForward = true;
                MovingBackward = false;
            }

            if (LastPosition.x > ActualPosition.x)
            {
                MovingForward = false;
                MovingBackward = true;
            }
            else if (LastPosition.x == ActualPosition.x)
            {
                MovingForward = false;
                MovingBackward = false;
            }
        }

        LastPosition.x = ActualPosition.x;
        LastPosition.z = ActualPosition.z;
        return zone;
        
    }

    public void PostAPI()
    {
        KartModel kart = new KartModel()
        {
            id = 1,
            fileId = int.Parse(fileId),
            time = Time.time,
            xPos = double.Parse(x.ToString()),
            yPos = double.Parse(y.ToString()),
            zPos = double.Parse(z.ToString()),
            leftSide = wallDetected[0],
            leftForward = wallDetected[1],
            centralForward = wallDetected[2],
            rightForward = wallDetected[3],
            rightSide = wallDetected[4],
            leftSideDistance = distanceToWall[0],
            leftForwardDistance = distanceToWall[1],
            centralForwardDistance = distanceToWall[2],
            rightForwardDistance = distanceToWall[3],
            rigthSideDistance = distanceToWall[4],
            zone = currZone,
            movingForward = MovingForward

        };
        string data = JsonConvert.SerializeObject(kart);
        var contentData = new StringContent(data, System.Text.Encoding.UTF8, "application/json");
        var URL = $"http://127.0.0.1:5000/kart";
        var client = new HttpClient();
        HttpResponseMessage response = client.PostAsync(URL, contentData).Result;
        var response_code = response.StatusCode;
        print(response_code);
       
    }

}