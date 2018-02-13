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
using System.Data.Common;
using System.Data.SqlClient;
using System.Reflection;
using log4net.Config;
using Mono.Data.Sqlite;
using MySQL.Data.MySqlClient;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using OpenMetaverse;
using OpenSim.Data.MSSQL;
using OpenSim.Data.MySQL;
using OpenSim.Data.SQLite;
using OpenSim.Framework;

namespace OpenSim.Data.Tests
{
#if NUNIT25

    [TestFixture(typeof(SqliteConnection), typeof(SQLiteInventoryStore), Description = "Inventory store tests (SQLite)")]
    [TestFixture(typeof(MySqlConnection), typeof(MySQLInventoryData), Description = "Inventory store tests (MySQL)")]
    [TestFixture(typeof(SqlConnection), typeof(MSSQLInventoryData), Description = "Inventory store tests (MS SQL Server)")]

#else

    [TestFixture(Description = "Inventory store tests (SQLite)")]
    public class SQLiteInventoryTests : InventoryTests<SqliteConnection, SQLiteInventoryStore>
    {
    }

    [TestFixture(Description = "Inventory store tests (MySQL)")]
    public class MySqlInventoryTests : InventoryTests<MySqlConnection, MySQLInventoryData>
    {
    }

    [TestFixture(Description = "Inventory store tests (MS SQL Server)")]
    public class MSSQLInventoryTests : InventoryTests<SqlConnection, MSSQLInventoryData>
    {
    }
#endif

    public class InventoryTests<TConn, TInvStore> : BasicDataServiceTest<TConn, TInvStore> where TConn : DbConnection, new() where TInvStore : class, IInventoryDataPlugin, new()
    {
        public IInventoryDataPlugin db;

        public UUID zero = UUID.Zero;

        public UUID folder1 = UUID.Random();
        public UUID folder2 = UUID.Random();
        public UUID folder3 = UUID.Random();
        public UUID owner1 = UUID.Random();
        public UUID owner2 = UUID.Random();
        public UUID owner3 = UUID.Random();

        public UUID item1 = UUID.Random();
        public UUID item2 = UUID.Random();
        public UUID item3 = UUID.Random();
        public UUID asset1 = UUID.Random();
        public UUID asset2 = UUID.Random();
        public UUID asset3 = UUID.Random();

        public string name1;
        public string name2 = "First Level folder";
        public string name3 = "First Level folder 2";
        public string niname1 = "My Shirt";
        public string iname1 = "Shirt";
        public string iname2 = "Text Board";
        public string iname3 = "No Pants Barrel";

        public InventoryTests(string conn) : base(conn)
        {
            name1 = "Root Folder for " + owner1.ToString();
        }

        public InventoryTests() : this("") { }

        protected override void InitService(object service)
        {
            ClearDB();
            db = (IInventoryDataPlugin)service;
            db.Initialise(m_connStr);
        }

        private void ClearDB()
        {
            DropTables("inventoryitems", "inventoryfolders");
            ResetMigrations("InventoryStore");
        }

        [Test]
        public void T001_LoadEmpty()
        {
            Assert.That(db.getInventoryFolder(zero), Is.Null);
            Assert.That(db.getInventoryFolder(folder1), Is.Null);
            Assert.That(db.getInventoryFolder(folder2), Is.Null);
            Assert.That(db.getInventoryFolder(folder3), Is.Null);

            Assert.That(db.getInventoryItem(zero), Is.Null);
            Assert.That(db.getInventoryItem(item1), Is.Null);
            Assert.That(db.getInventoryItem(item2), Is.Null);
            Assert.That(db.getInventoryItem(item3), Is.Null);

            Assert.That(db.getUserRootFolder(zero), Is.Null);
            Assert.That(db.getUserRootFolder(owner1), Is.Null);
        }

        // 01x - folder tests
        [Test]
        public void T010_FolderNonParent()
        {
            InventoryFolderBase f1 = NewFolder(folder2, folder1, owner1, name2);

            // the folder will go in
            db.addInventoryFolder(f1);
            InventoryFolderBase f1a = db.getUserRootFolder(owner1);
            Assert.That(f1a, Is.Null);
        }

        [Test]
        public void T011_FolderCreate()
        {
            InventoryFolderBase f1 = NewFolder(folder1, zero, owner1, name1);

            // succeed with true
            db.addInventoryFolder(f1);
            InventoryFolderBase f1a = db.getUserRootFolder(owner1);
            Assert.That(folder1, Is.EqualTo(f1a.ID), "Assert.That(folder1, Is.EqualTo(f1a.ID))");
            Assert.That(name1, Text.Matches(f1a.Name), "Assert.That(name1, Text.Matches(f1a.Name))");
        }

        // we now have the following tree
        // folder1
        //   +--- folder2
        //   +--- folder3

        [Test]
        public void T012_FolderList()
        {
            InventoryFolderBase f2 = NewFolder(folder3, folder1, owner1, name3);
            db.addInventoryFolder(f2);

            Assert.That(db.getInventoryFolders(zero).Count, Is.EqualTo(1), "Assert.That(db.getInventoryFolders(zero).Count, Is.EqualTo(1))");
            Assert.That(db.getInventoryFolders(folder1).Count, Is.EqualTo(2), "Assert.That(db.getInventoryFolders(folder1).Count, Is.EqualTo(2))");
            Assert.That(db.getInventoryFolders(folder2).Count, Is.EqualTo(0), "Assert.That(db.getInventoryFolders(folder2).Count, Is.EqualTo(0))");
            Assert.That(db.getInventoryFolders(folder3).Count, Is.EqualTo(0), "Assert.That(db.getInventoryFolders(folder3).Count, Is.EqualTo(0))");
            Assert.That(db.getInventoryFolders(UUID.Random()).Count, Is.EqualTo(0), "Assert.That(db.getInventoryFolders(UUID.Random()).Count, Is.EqualTo(0))");
        }

        [Test]
        public void T013_FolderHierarchy()
        {
            int n = db.getFolderHierarchy(zero).Count;  // (for dbg - easier to see what's returned)
            Assert.That(n, Is.EqualTo(0), "Assert.That(db.getFolderHierarchy(zero).Count, Is.EqualTo(0))");
            n = db.getFolderHierarchy(folder1).Count;
            Assert.That(n, Is.EqualTo(2), "Assert.That(db.getFolderHierarchy(folder1).Count, Is.EqualTo(2))");
            Assert.That(db.getFolderHierarchy(folder2).Count, Is.EqualTo(0), "Assert.That(db.getFolderHierarchy(folder2).Count, Is.EqualTo(0))");
            Assert.That(db.getFolderHierarchy(folder3).Count, Is.EqualTo(0), "Assert.That(db.getFolderHierarchy(folder3).Count, Is.EqualTo(0))");
            Assert.That(db.getFolderHierarchy(UUID.Random()).Count, Is.EqualTo(0), "Assert.That(db.getFolderHierarchy(UUID.Random()).Count, Is.EqualTo(0))");
        }

        [Test]
        public void T014_MoveFolder()
        {
            InventoryFolderBase f2 = db.getInventoryFolder(folder2);
            f2.ParentID = folder3;
            db.moveInventoryFolder(f2);

            Assert.That(db.getInventoryFolders(zero).Count, Is.EqualTo(1), "Assert.That(db.getInventoryFolders(zero).Count, Is.EqualTo(1))");
            Assert.That(db.getInventoryFolders(folder1).Count, Is.EqualTo(1), "Assert.That(db.getInventoryFolders(folder1).Count, Is.EqualTo(1))");
            Assert.That(db.getInventoryFolders(folder2).Count, Is.EqualTo(0), "Assert.That(db.getInventoryFolders(folder2).Count, Is.EqualTo(0))");
            Assert.That(db.getInventoryFolders(folder3).Count, Is.EqualTo(1), "Assert.That(db.getInventoryFolders(folder3).Count, Is.EqualTo(1))");
            Assert.That(db.getInventoryFolders(UUID.Random()).Count, Is.EqualTo(0), "Assert.That(db.getInventoryFolders(UUID.Random()).Count, Is.EqualTo(0))");
        }

        [Test]
        public void T015_FolderHierarchy()
        {
            Assert.That(db.getFolderHierarchy(zero).Count, Is.EqualTo(0), "Assert.That(db.getFolderHierarchy(zero).Count, Is.EqualTo(0))");
            Assert.That(db.getFolderHierarchy(folder1).Count, Is.EqualTo(2), "Assert.That(db.getFolderHierarchy(folder1).Count, Is.EqualTo(2))");
            Assert.That(db.getFolderHierarchy(folder2).Count, Is.EqualTo(0), "Assert.That(db.getFolderHierarchy(folder2).Count, Is.EqualTo(0))");
            Assert.That(db.getFolderHierarchy(folder3).Count, Is.EqualTo(1), "Assert.That(db.getFolderHierarchy(folder3).Count, Is.EqualTo(1))");
            Assert.That(db.getFolderHierarchy(UUID.Random()).Count, Is.EqualTo(0), "Assert.That(db.getFolderHierarchy(UUID.Random()).Count, Is.EqualTo(0))");
        }

        // Item tests
        [Test]
        public void T100_NoItems()
        {
            Assert.That(db.getInventoryInFolder(zero).Count, Is.EqualTo(0), "Assert.That(db.getInventoryInFolder(zero).Count, Is.EqualTo(0))");
            Assert.That(db.getInventoryInFolder(folder1).Count, Is.EqualTo(0), "Assert.That(db.getInventoryInFolder(folder1).Count, Is.EqualTo(0))");
            Assert.That(db.getInventoryInFolder(folder2).Count, Is.EqualTo(0), "Assert.That(db.getInventoryInFolder(folder2).Count, Is.EqualTo(0))");
            Assert.That(db.getInventoryInFolder(folder3).Count, Is.EqualTo(0), "Assert.That(db.getInventoryInFolder(folder3).Count, Is.EqualTo(0))");
        }

        // TODO: Feeding a bad inventory item down the data path will
        // crash the system.  This is largely due to the builder
        // routines.  That should be fixed and tested for.
        [Test]
        public void T101_CreatItems()
        {
            db.addInventoryItem(NewItem(item1, folder3, owner1, iname1, asset1));
            db.addInventoryItem(NewItem(item2, folder3, owner1, iname2, asset2));
            db.addInventoryItem(NewItem(item3, folder3, owner1, iname3, asset3));
            Assert.That(db.getInventoryInFolder(folder3).Count, Is.EqualTo(3), "Assert.That(db.getInventoryInFolder(folder3).Count, Is.EqualTo(3))");
        }

        [Test]
        public void T102_CompareItems()
        {
            InventoryItemBase i1 = db.getInventoryItem(item1);
            InventoryItemBase i2 = db.getInventoryItem(item2);
            InventoryItemBase i3 = db.getInventoryItem(item3);
            Assert.That(i1.Name, Is.EqualTo(iname1), "Assert.That(i1.Name, Is.EqualTo(iname1))");
            Assert.That(i2.Name, Is.EqualTo(iname2), "Assert.That(i2.Name, Is.EqualTo(iname2))");
            Assert.That(i3.Name, Is.EqualTo(iname3), "Assert.That(i3.Name, Is.EqualTo(iname3))");
            Assert.That(i1.Owner, Is.EqualTo(owner1), "Assert.That(i1.Owner, Is.EqualTo(owner1))");
            Assert.That(i2.Owner, Is.EqualTo(owner1), "Assert.That(i2.Owner, Is.EqualTo(owner1))");
            Assert.That(i3.Owner, Is.EqualTo(owner1), "Assert.That(i3.Owner, Is.EqualTo(owner1))");
            Assert.That(i1.AssetID, Is.EqualTo(asset1), "Assert.That(i1.AssetID, Is.EqualTo(asset1))");
            Assert.That(i2.AssetID, Is.EqualTo(asset2), "Assert.That(i2.AssetID, Is.EqualTo(asset2))");
            Assert.That(i3.AssetID, Is.EqualTo(asset3), "Assert.That(i3.AssetID, Is.EqualTo(asset3))");
        }

        [Test]
        public void T103_UpdateItem()
        {
            // TODO: probably shouldn't have the ability to have an
            // owner of an item in a folder not owned by the user
            InventoryItemBase i1 = db.getInventoryItem(item1);
            i1.Name = niname1;
            i1.Description = niname1;
            i1.Owner = owner2;
            db.updateInventoryItem(i1);

            i1 = db.getInventoryItem(item1);
            Assert.That(i1.Name, Is.EqualTo(niname1), "Assert.That(i1.Name, Is.EqualTo(niname1))");
            Assert.That(i1.Description, Is.EqualTo(niname1), "Assert.That(i1.Description, Is.EqualTo(niname1))");
            Assert.That(i1.Owner, Is.EqualTo(owner2), "Assert.That(i1.Owner, Is.EqualTo(owner2))");
        }

        [Test]
        public void T104_RandomUpdateItem()
        {
            PropertyScrambler<InventoryFolderBase> folderScrambler =
                new PropertyScrambler<InventoryFolderBase>()
                    .DontScramble(x => x.Owner)
                    .DontScramble(x => x.ParentID)
                    .DontScramble(x => x.ID);
            UUID owner = UUID.Random();
            UUID folder = UUID.Random();
            UUID rootId = UUID.Random();
            UUID rootAsset = UUID.Random();
            InventoryFolderBase f1 = NewFolder(folder, zero, owner, name1);
            folderScrambler.Scramble(f1);

            db.addInventoryFolder(f1);
            InventoryFolderBase f1a = db.getUserRootFolder(owner);
            Assert.That(f1a, Constraints.PropertyCompareConstraint(f1));

            folderScrambler.Scramble(f1a);

            db.updateInventoryFolder(f1a);

            InventoryFolderBase f1b = db.getUserRootFolder(owner);
            Assert.That(f1b, Constraints.PropertyCompareConstraint(f1a));

            //Now we have a valid folder to insert into, we can insert the item.
            PropertyScrambler<InventoryItemBase> inventoryScrambler =
                new PropertyScrambler<InventoryItemBase>()
                    .DontScramble(x => x.ID)
                    .DontScramble(x => x.AssetID)
                    .DontScramble(x => x.Owner)
                    .DontScramble(x => x.Folder);
            InventoryItemBase root = NewItem(rootId, folder, owner, iname1, rootAsset);
            inventoryScrambler.Scramble(root);
            db.addInventoryItem(root);

            InventoryItemBase expected = db.getInventoryItem(rootId);
            Assert.That(expected, Constraints.PropertyCompareConstraint(root)
                                    .IgnoreProperty(x => x.InvType)
                                    .IgnoreProperty(x => x.CreatorIdAsUuid)
                                    .IgnoreProperty(x => x.Description)
                                    .IgnoreProperty(x => x.CreatorId));

            inventoryScrambler.Scramble(expected);
            db.updateInventoryItem(expected);

            InventoryItemBase actual = db.getInventoryItem(rootId);
            Assert.That(actual, Constraints.PropertyCompareConstraint(expected)
                                    .IgnoreProperty(x => x.InvType)
                                    .IgnoreProperty(x => x.CreatorIdAsUuid)
                                    .IgnoreProperty(x => x.Description)
                                    .IgnoreProperty(x => x.CreatorId));
        }

        [Test]
        public void T999_StillNull()
        {
            // After all tests are run, these should still return no results
            Assert.That(db.getInventoryFolder(zero), Is.Null);
            Assert.That(db.getInventoryItem(zero), Is.Null);
            Assert.That(db.getUserRootFolder(zero), Is.Null);
            Assert.That(db.getInventoryInFolder(zero).Count, Is.EqualTo(0), "Assert.That(db.getInventoryInFolder(zero).Count, Is.EqualTo(0))");
        }

        private InventoryItemBase NewItem(UUID id, UUID parent, UUID owner, string name, UUID asset)
        {
            InventoryItemBase i = new InventoryItemBase();
            i.ID = id;
            i.Folder = parent;
            i.Owner = owner;
            i.CreatorId = owner.ToString();
            i.Name = name;
            i.Description = name;
            i.AssetID = asset;
            return i;
        }

        private InventoryFolderBase NewFolder(UUID id, UUID parent, UUID owner, string name)
        {
            InventoryFolderBase f = new InventoryFolderBase();
            f.ID = id;
            f.ParentID = parent;
            f.Owner = owner;
            f.Name = name;
            return f;
        }
    }
}