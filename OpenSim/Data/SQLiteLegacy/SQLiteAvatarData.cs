/// <summary>
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
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Threading;
using log4net;
using OpenMetaverse;
using OpenSim.Framework;
using Mono.Data.SqliteClient;

namespace OpenSim.Data.SQLiteLegacy
{
    /// <summary>
    ///     A SQLite Interface for Avatar Data
    /// </summary>
    public class SQLiteAvatarData : SQLiteGenericTableHandler<AvatarBaseData>, IAvatarData
    {
        private static readonly ILog m_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public SQLiteAvatarData(string connectionString, string realm) : base(connectionString, realm, "Avatar")
        {
        }

        public bool Delete(UUID principalID, string name)
        {
            SqliteCommand cmd = new SqliteCommand();

            cmd.CommandText = String.Format("delete from {0} where `PrincipalID` = :PrincipalID and `Name` = :Name", m_Realm);
            cmd.Parameters.Add(":PrincipalID", principalID.ToString());
            cmd.Parameters.Add(":Name", name);

            try
            {
                if (ExecuteNonQuery(cmd, m_Connection) > 0)
                {
                    return true;
                }

                return false;
            }
            finally
            {
                CloseCommand(cmd);
            }
        }
    }
}