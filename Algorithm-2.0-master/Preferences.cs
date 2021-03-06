﻿using Newtonsoft.Json;
using System.Collections.Generic;

namespace Scheduler {
    public class Preferences {
        #region NOTES
        //------------------------------------------------------------------------------
        // This Class utilizes a JSON string generated by a SQL query from ParameterSet
        // to generate the preferences to be used by the scheduler. Should Preferences 
        // be expanded in the database then this class and the ParameterSet class MUST be
        // altered to accommodate the changes.
        //------------------------------------------------------------------------------
        #endregion

        #region VARIABLES
        //------------------------------------------------------------------------------
        // Variables listed other than List<ParameterSet> are different than the types 
        // of their counterparts in the database and needed further deserialization or 
        // interpretation. 
        //------------------------------------------------------------------------------
        private string JsonString; //just a local copy, not used for anything important. Good for checking if what you're getting from the DB is correct
        private DBConnection dbHit; //boop...deboop...lets query that database shall we?
        private List<ParameterSet> prefs; //JSON Deserializer needs this to be in a collection even though there will only be one element
        private List<Job> priors; //used as a collection for completed courses, no longer used as of version 2.2
        bool summerIntent; //Since it's currently a VARCHAR in the database this is what we need to change it to

        private class CourseNumbers //Used as a deserialized structure for both CompletedCourses and PlacementCourses
        {
            [JsonProperty]
            private string CourseNumber { get; set; }

            public string returnCourse()
            {
                return CourseNumber;
            }
        }
        #endregion

        #region Constructors
        //------------------------------------------------------------------------------
        // default constructor
        // Only use is to provide the terminating character of an empty SQL query. There
        // is nothing else that is done, signifying that preferences need to be passed
        // for the scheduling algorithm to be ran.
        //------------------------------------------------------------------------------
        public Preferences() {
            JsonString = ";";
        }

        //------------------------------------------------------------------------------
        // Creates Preferences from a JSON string that is ready to be deserialized
        //------------------------------------------------------------------------------
        public Preferences(string JsonInput)
        {
            JsonString = JsonInput;
            Deserialize();
        }

        //------------------------------------------------------------------------------
        // Queries the database with the ID of the ParameterSet that contains the desired
        // preferences.
        //------------------------------------------------------------------------------
        public Preferences(int parameterID)
        {
            dbHit = new DBConnection();
            JsonString = "";
            JsonString = dbHit.ExecuteToString("select * from ParameterSet where ParameterSetID =" + parameterID + "for JSON AUTO;");
            Deserialize();
            //SetPriors();
            determineSummer();
        }
        #endregion

        #region JsonDeserializer
        //------------------------------------------------------------------------------
        // Deserializes the string into the ParameterSet object, which is a item-by-item
        // representation of the corresponding table in the database
        //------------------------------------------------------------------------------
        private void Deserialize()
        {
            prefs = new List<ParameterSet>();
            prefs = JsonConvert.DeserializeObject<List<ParameterSet>>(JsonString, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Include });
        }
        #endregion

        #region Getters
        public List<Job> getPriors()
        {
            return priors;
        }
        public int getMajor()
        {
            return prefs[0].getMajor();
        }

        public int getSchool()
        {
            return prefs[0].getSchool();
        }

        public int getQuarters()
        {
            return prefs[0].getMaxQuarters();
        }

        public bool getSummer()
        {
            return summerIntent;
        }

        public int getCreditsPerQuarter() 
        {
            return prefs[0].getCreditsPerQuarter();
        }

        public int getCoreCredits()
        {
            return prefs[0].getCoreCourse();
        }
        #endregion

        #region Data Interpretation and additional Deserialization
        ////------------------------------------------------------------------------------
        //// Provides the overall starting point based off placement courses and completed
        //// courses.
        ////------------------------------------------------------------------------------
        //private void SetPriors()
        //{
        //    priors = new List<Job>();
        //    addTopriors(prefs[0].getCompleted());
        //    addTopriors(prefs[0].getPlacement());
        //}

        ////------------------------------------------------------------------------------
        //// Deserializes the given string into a collection of CourseNumbers which are 
        //// then added into the list of courses to be used as a starting point.
        ////------------------------------------------------------------------------------
        //private void addTopriors(string passed)
        //{
        //    if (passed != null)
        //    {
        //        List<CourseNumbers> courses = JsonConvert.DeserializeObject<List<CourseNumbers>>(passed);
        //        for (int i = 0; i < courses.Count; i++)
        //        {
        //            DataTable courseID = dbHit.ExecuteToDT("select CourseID from Course where CourseNumber = '" + courses[i].returnCourse() + "' ;");
        //            Job tempJob = new Job((int)courseID.Rows[0].ItemArray[0]);
        //            tempJob.SetScheduled(true);
        //            priors.Add(tempJob);
        //        }
        //    }
        //}

        //------------------------------------------------------------------------------
        // Mainly used because the database represenetation of the summer preference
        // is set as a varchar which doesnt play nice with booleans. So, if the first 
        // character begins with 'Y' or 'y' then this will read as true.
        // 
        // This could be done better, but so could the representation of the preference 
        // in the database.
        //------------------------------------------------------------------------------
        private void determineSummer()
        {
            char test = prefs[0].getSummer()[0];
            //Console.WriteLine(test);
            if (test == 'Y' || test == 'y')
            {
                summerIntent = true;
            }
            //Console.WriteLine(summerIntent);
        }
        
        #endregion
    }
}
