﻿/// <summary>
///     Copyright (c) Contributors, http://opensimulator.org/
///     See CONTRIBUTORS.TXT for a full list of copyright holders.
///     For an explanation of the license of each contributor and the content it 
///     covers please see the Licenses directory.
/// 
///     Redistribution and use in source and binary forms, with or without
///     modification, are permitted provided that the following conditions are met:
///         * Redistributions of source code must retain the above copyright
///         notice, this list of conditions and the following disclaimer.
///         * Redistributions in binary form must reproduce the above copyright
///         notice, this list of conditions and the following disclaimer in the
///         documentation and/or other materials provided with the distribution.
///         * Neither the name of the OpenSim Project nor the
///         names of its contributors may be used to endorse or promote products
///         derived from this software without specific prior written permission.
/// 
///     THIS SOFTWARE IS PROVIDED BY THE DEVELOPERS ``AS IS'' AND ANY
///     EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
///     WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
///     DISCLAIMED. IN NO EVENT SHALL THE CONTRIBUTORS BE LIABLE FOR ANY
///     DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
///     (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
///     LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
///     ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
///     (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
///     SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
/// </summary>

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using OpenMetaverse;
using OpenSim.Framework;

namespace OpenSim.Data.MSSQL
{
    public class MSSQLUserAccountData : MSSQLGenericTableHandler<UserAccountData>, IUserAccountData
    {
        public MSSQLUserAccountData(string connectionString, string realm) : base(connectionString, realm, "UserAccount")
        {
        }

        public UserAccountData[] GetUsers(UUID scopeID, string query)
        {
            string[] words = query.Split(new char[] { ' ' });

            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].Length < 3)
                {
                    if (i != words.Length - 1)
                    {
                        Array.Copy(words, i + 1, words, i, words.Length - i - 1);
                    }

                    Array.Resize(ref words, words.Length - 1);
                }
            }

            if (words.Length == 0)
            {
                return new UserAccountData[0];
            }

            if (words.Length > 2)
            {
                return new UserAccountData[0];
            }

            using (SqlConnection conn = new SqlConnection(m_ConnectionString))

            using (SqlCommand cmd = new SqlCommand())
            {
                if (words.Length == 1)
                {
                    cmd.CommandText = String.Format("select * from {0} where ([ScopeID]=@ScopeID or [ScopeID]='00000000-0000-0000-0000-000000000000') and ([FirstName] like @search or [LastName] like @search)", m_Realm);
                    cmd.Parameters.Add(m_database.CreateParameter("@scopeID", scopeID));
                    cmd.Parameters.Add(m_database.CreateParameter("@search", "%" + words[0] + "%"));
                }
                else
                {
                    cmd.CommandText = String.Format("select * from {0} where ([ScopeID]=@ScopeID or [ScopeID]='00000000-0000-0000-0000-000000000000') and ([FirstName] like @searchFirst or [LastName] like @searchLast)", m_Realm);
                    cmd.Parameters.Add(m_database.CreateParameter("@searchFirst", "%" + words[0] + "%"));
                    cmd.Parameters.Add(m_database.CreateParameter("@searchLast", "%" + words[1] + "%"));
                    cmd.Parameters.Add(m_database.CreateParameter("@ScopeID", scopeID.ToString()));
                }

                return DoQuery(cmd);
            }
        }
    }
}