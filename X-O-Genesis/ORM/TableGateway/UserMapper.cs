﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using PasswordHash;

namespace PetvetPOS_Inventory_System
{
    public class UserMapper: DatabaseMapper
    {
        private const int ACTIVE = 1;
        public UserMapper(MySqlDB db): base(db)
        {
            tableName = "user_tbl";
            id = "id";
            fieldsname = new string[] {
                "id",
                "password",
                "access_level",
                "active",
                "create_time",
                "session_status",
                "fallbackid",
                "fallbackans",
            };
        }


        public List<string> getListOfActiveUsername()
        {
            return getList("id", "active = 1");
        }

        public int getSqueryIndexFromID(string id) 
        {
            return (int)readScalar("fallbackid", "id = '" + id + "'");
        }

        public List<string> getListOfAllUsername()
        {
            return getList("id");
        }

        public string createUser(User user)
        {
            return insertValues(user.UserId, PasswordHash.PasswordHash.CreateHash(user.Password), user.getUserLevel().ToString(), ACTIVE , "NOW()", 0, user.Squery, PasswordHash.PasswordHash.CreateHash(user.FBAnswer));
        }

        public bool inactivateUser(User user)
        {
            string condition = String.Format("id = '{0}'", user);
            return update(updateSet(condition, "active = 0"));
        }
        public User getUserFromId(string id)
        {
            return new User(getEntityFromId(id));
        }

        public User validate(string user_id, string password)
        {
            List<string> usernames = getListOfActiveUsername();
            if (usernames.Contains(user_id))
            {
                try
                {
                    string condition = String.Format("id = '{0}'", user_id);
                    string correctHash = (string)readScalar("password", condition);
                    bool isValid = PasswordHash.PasswordHash.ValidatePassword(password, correctHash);
                    if (isValid)
                    {
                        condition = String.Format("id = '{0}' AND password = '{1}' AND session_status = 0", user_id, correctHash);
                        return getUserFromId((string)readScalar("id", condition));                      
                    }
                }
                catch (Exception ex)
                {
                    ErrorLog.Log(ex);
                    return null;
                }
            }
            return null;
        }

        public bool getSQAnswer(string userName, string sqans)
        {
            try
            {
                string condition = String.Format("id = '{0}'", userName);
                string correctHash = (string)readScalar("fallbackans", condition);
                bool isValid = PasswordHash.PasswordHash.ValidatePassword(sqans, correctHash);
                if (isValid)
                    return true;
                else
                    return false;               
            }
            catch (Exception ex)
            {
                ErrorLog.Log(ex);
                return false;
            } 
        }

        public bool login(string user_id)
        {
            int login_state = 1;
            string condition = string.Format("id = '{0}'", user_id);
            string session_state = string.Format("session_status = {0}", login_state);
            return update(updateSet(condition, session_state));
        }

        public bool changePass(string userName, string newPass)
        {
            string condition = String.Format("id = '{0}'", userName);
            string newpw = String.Format("password = '{0}'", PasswordHash.PasswordHash.CreateHash(newPass));
            return update(updateSet(condition, newpw));
        }

        public bool isAlreadyLogin(User user)
        {
            string condition = string.Format("id = '{0}' && session_status = 1", user.UserId);
            object foo = readScalar("session_status", condition);

            if (foo != null)
                return true;
            else
                return false;
       
        }
        public bool logout(string user_id)
        {
            int login_state = 0;
            string condition = string.Format("id = '{0}'", user_id);
            string session_state = string.Format("session_status = {0}", login_state);
            return update(updateSet(condition, session_state));
        }

        public bool updateSquery(User oldSquery, User newSquery)
        {
            if (oldSquery.UserId != newSquery.UserId)
                return false;
            int updateSquery;
            string updateAns;

            if (!string.IsNullOrWhiteSpace(Convert.ToString(newSquery.Squery)))
                updateSquery = Convert.ToInt32(String.Format("fallbackid = {0}", newSquery.Squery));
            if (!string.IsNullOrWhiteSpace(newSquery.FBAnswer))
                updateAns = String.Format("fallbackans = '{0}'", newSquery.FBAnswer);

            string condition = String.Format("id = '{0}'", newSquery.UserId);
            //What to do when parameters contains non-string inputs (update)?
            //return update(updateSet(condition, updateSquery, updateAns));            
            return false;
        }


        public bool checkUsername(string userName)
        {
            string condition = String.Format("id = '{0}'", userName);
            object foo = readScalar("id", condition);
            if (foo != null)
                return true;
            else
                return false;
        }


    }
}
