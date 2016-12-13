﻿using FFXIVClassic_World_Server.DataObjects.Group;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIVClassic_World_Server
{
    class RetainerGroupManager
    {
        private Server mServer;
        private Object mGroupLockReference;
        private Dictionary<ulong, Group> mCurrentWorldGroupsReference;
        private Dictionary<uint, RetainerGroup> mRetainerGroupList = new Dictionary<uint, RetainerGroup>();

        public RetainerGroupManager(Server server, Object groupLock, Dictionary<ulong, Group> worldGroupList)
        {
            mServer = server;
            mGroupLockReference = groupLock;
            mCurrentWorldGroupsReference = worldGroupList;
        }

        public RetainerGroup GetRetainerGroup(uint charaId)
        {
            if (!mRetainerGroupList.ContainsKey(charaId))            
                return LoadRetainerGroup(charaId);            
            else
                return mRetainerGroupList[charaId];
        }

        private RetainerGroup LoadRetainerGroup(uint charaId)
        {
            lock(mGroupLockReference)
            {
                ulong groupId = mServer.GetGroupIndex();
                RetainerGroup retainerGroup = new RetainerGroup(groupId, charaId);

                Dictionary<uint, RetainerGroupMember> members = Database.GetRetainers(charaId);
                if (members == null)
                    return null;

                retainerGroup.members = members;                
                mRetainerGroupList.Add(charaId, retainerGroup);
                mCurrentWorldGroupsReference.Add(groupId, retainerGroup);

                mServer.IncrementGroupIndex();

                return retainerGroup;
            }
        }

        public void AddRetainerToGroup(ulong charaId, uint retainerId)
        {

        }

        public void RemoveRetainerFromGroup(ulong charaId, uint retainerId)
        {

        }        
    }
}