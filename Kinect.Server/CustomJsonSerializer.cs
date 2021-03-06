﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Json;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using Microsoft.Kinect;


namespace Kinect.Server
{
    /// <summary>
    /// Serializes messages to clients to JSON fromat.
    /// </summary>
    public static class CustomJsonSerializer
    {
        #region protocol for home page - when alarm rings
        [DataContract]
        class JSONHomePage
        {
            [DataMember(Name = "page")]
            public String Page { get; set; }

            [DataMember(Name = "operation")]
            public String Operation { get; set; }

            [DataMember(Name = "data")]
            public JSONHomePageData Data { get; set; }
        }

        [DataContract]
        class JSONHomePageData
        {
            [DataMember(Name = "skeleton")]
            public JSONSkeleton Skeleton { get; set; }

            [DataMember(Name = "alarmOn")]
            public Boolean AlarmOn { get; set; }

            [DataMember(Name = "avgFrame")]
            public double AvgFrames { get; set; }

            [DataMember(Name = "curFrame")]
            public int CurrFrames { get; set; }

            [DataMember(Name = "excRemaining")]
            public List<String> ExcRemaining { get; set; }
        }

        [DataContract]
        class JSONSkeleton
        {
            [DataMember(Name = "id")]
            public string ID { get; set; }

            [DataMember(Name = "joints")]
            public List<JSONJoint> Joints { get; set; }
        }

        [DataContract]
        class JSONJoint
        {
            [DataMember(Name = "name")]
            public string Name { get; set; }

            [DataMember(Name = "x")]
            public double X { get; set; }

            [DataMember(Name = "y")]
            public double Y { get; set; }
        }
        #endregion
        
        [DataContract]
        class JSONObsSeqs
        {
            [DataMember(Name = "observationSequences")]
            public List<List<Point>> obsSeq { get; set; }
        }        

        public static string SerializeObservationSequences(List<List<Point>> observationSequences){
            JSONObsSeqs obsSeqs = new JSONObsSeqs
            {
                obsSeq = observationSequences
            };
            return Serialize(obsSeqs);
        }

        public static string SerializeHomePageData(Body skeleton, Dictionary<JointType, Point> jointPoints, Boolean alarmOn, double avgFrames, int currFrames, List<String> excRemaining)
        {
            JSONSkeleton jsonSkeleton = new JSONSkeleton
            {
                ID = skeleton.TrackingId.ToString(),
                Joints = new List<JSONJoint>()
            };

            foreach (JointType jointType in jointPoints.Keys)
            {
                jsonSkeleton.Joints.Add(new JSONJoint
                {
                    Name = jointType.ToString().ToLower(),
                    X = jointPoints[jointType].X,
                    Y = jointPoints[jointType].Y
                });
            }

            JSONHomePageData jsonHomePageData = new JSONHomePageData
            {
                Skeleton = jsonSkeleton,
                AlarmOn = alarmOn,
                AvgFrames = avgFrames,
                CurrFrames = currFrames,
                ExcRemaining = excRemaining
            };

            JSONHomePage jsonHomePage = new JSONHomePage{
                Page = "home",
                Operation = "default",
                Data = jsonHomePageData
            };


            return Serialize(jsonHomePage);
        }

        /// <summary>
        /// Serializes an object to JSON.
        /// </summary>
        /// <param name="obj">The specified object.</param>
        /// <returns>A JSON representation of the object.</returns>
        private static string Serialize(object obj)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(obj.GetType());

            using (MemoryStream ms = new MemoryStream())
            {
                serializer.WriteObject(ms, obj);

                return Encoding.Default.GetString(ms.ToArray());
            }
        }
    }
}
