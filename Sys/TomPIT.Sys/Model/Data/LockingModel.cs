using System;
using System.Collections.Concurrent;
using TomPIT.Data;

namespace TomPIT.Sys.Model.Data
{
   public class LockingModel
   {
      private readonly ConcurrentDictionary<string, ILock> _locks = new();
      private readonly ConcurrentDictionary<Guid, ILock> _lockKeys = new();


      public ILock Lock(string entity, TimeSpan timeout)
      {
         //The response of null means a lock could not be acquired

         //If lock exists
         if (_locks.TryGetValue(entity, out var lockEntity))
         {
            if (lockEntity.Timeout <= DateTime.UtcNow)
            {
               //If timed out, attempt to remove
               _locks.TryRemove(entity, out _);
               _lockKeys.TryRemove(lockEntity.UnlockKey, out _);
            }
            else
               //Lock exists and has not timed out, return null
               return null;
         }

         //Attempt to add, if unsuccessful, return null, somebody beat us to it
         var newLock = new EphemeralLock(entity, timeout);

         if (_locks.TryAdd(entity, newLock))
            _lockKeys.AddOrUpdate(newLock.UnlockKey, newLock, (_, _) => newLock);
         else
            return null;

         return newLock;
      }

      public void Ping(Guid unlockKey, TimeSpan timeout)
      {
         if (_lockKeys.TryGetValue(unlockKey, out var oldLock))
         {
            var newLock = new EphemeralLock(oldLock.Entity, timeout) { UnlockKey = oldLock.UnlockKey };

            if (_lockKeys.TryUpdate(unlockKey, newLock, oldLock))
               _locks.TryUpdate(oldLock.Entity, newLock, oldLock);
         }
      }

      public void Unlock(Guid unlockKey)
      {
         if (_lockKeys.TryRemove(unlockKey, out var oldLock))
            _locks.TryRemove(oldLock.Entity, out _);
      }

      internal class EphemeralLock : ILock
      {
         public EphemeralLock(string entity, TimeSpan timeout)
         {
            if (string.IsNullOrWhiteSpace(entity))
            {
               entity = Guid.NewGuid().ToString();
            }

            this.Entity = entity;
            this.Timeout = DateTime.UtcNow.Add(timeout);
         }

         public string Entity { get; private set; }

         public Guid UnlockKey { get; init; } = Guid.NewGuid();

         public DateTime Timeout { get; private set; } = DateTime.UtcNow;
      }
   }


}
