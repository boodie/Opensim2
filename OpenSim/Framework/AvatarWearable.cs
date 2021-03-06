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
using System.Runtime.Serialization;
using System.Security.Permissions;
using OpenMetaverse;

namespace OpenSim.Framework
{
    public class AvatarWearable
    {
        public static readonly UUID DEFAULT_BODY_ITEM = new UUID("66c41e39-38f9-f75a-024e-585989bfaba9");
        public static readonly UUID DEFAULT_BODY_ASSET = new UUID("66c41e39-38f9-f75a-024e-585989bfab73");

        public static readonly UUID DEFAULT_HAIR_ITEM = new UUID("d342e6c1-b9d2-11dc-95ff-0800200c9a66");
        public static readonly UUID DEFAULT_HAIR_ASSET = new UUID("d342e6c0-b9d2-11dc-95ff-0800200c9a66");

        public static readonly UUID DEFAULT_SKIN_ITEM = new UUID("77c41e39-38f9-f75a-024e-585989bfabc9");
        public static readonly UUID DEFAULT_SKIN_ASSET = new UUID("77c41e39-38f9-f75a-024e-585989bbabbb");

        public static readonly UUID DEFAULT_SHIRT_ITEM = new UUID("77c41e39-38f9-f75a-0000-585989bf0000");
        public static readonly UUID DEFAULT_SHIRT_ASSET = new UUID("00000000-38f9-1111-024e-222222111110");

        public static readonly UUID DEFAULT_PANTS_ITEM = new UUID("77c41e39-38f9-f75a-0000-5859892f1111");
        public static readonly UUID DEFAULT_PANTS_ASSET = new UUID("00000000-38f9-1111-024e-222222111120");

        public UUID AssetID;
        public UUID ItemID;

        public AvatarWearable()
        {
        }

        public AvatarWearable(UUID itemId, UUID assetId)
        {
            AssetID = assetId;
            ItemID = itemId;
        }

        public static AvatarWearable[] DefaultWearables
        {
            get
            {
                AvatarWearable[] defaultWearables = new AvatarWearable[13]; //should be 13 of these

                for (int i = 0; i < 13; i++)
                {
                    defaultWearables[i] = new AvatarWearable();
                }

                // Body
                defaultWearables[0].ItemID = DEFAULT_BODY_ITEM;
                defaultWearables[0].AssetID = DEFAULT_BODY_ASSET;

                // Hair
                defaultWearables[2].ItemID = DEFAULT_HAIR_ITEM;
                defaultWearables[2].AssetID = DEFAULT_HAIR_ASSET;

                // Skin
                defaultWearables[1].ItemID = DEFAULT_SKIN_ITEM;
                defaultWearables[1].AssetID = DEFAULT_SKIN_ASSET;

                // Shirt
                defaultWearables[4].ItemID = DEFAULT_SHIRT_ITEM;
                defaultWearables[4].AssetID = DEFAULT_SHIRT_ASSET;

                // Pants
                defaultWearables[5].ItemID = DEFAULT_PANTS_ITEM;
                defaultWearables[5].AssetID = DEFAULT_PANTS_ASSET;

                return defaultWearables;
            }
        }
    }
}