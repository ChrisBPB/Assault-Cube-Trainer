using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assault_Cube_Trainer
{
    /**
     * Represents any entity with an X Y and Z position in the game
     */ 
    public class LocatableEntity
    {

        public struct Offsets
        {
            public int xPos, yPos, zPos;

            public Offsets(int xPos, int yPos, int zPos)
            {
                this.xPos = xPos;
                this.yPos = yPos;
                this.zPos = zPos;

            }
        }

        public Offsets offsets = new Offsets(
                0x04,
                0x08,
                0x0C               
            );

        public int baseAddress; //the base address of the entity (base of module + offset!)

        public float xPos, yPos, zPos;


        public Memory pm; //our memory management object

        /**
         * Constructor for multilevel pointer.
         */ 
        public LocatableEntity(int baseAddress, int[] multiLevel, Memory pm)
        {
            this.baseAddress = pm.ReadMultiLevelPointer(baseAddress, 4, multiLevel);
            this.pm = pm;
        }

        /**
         * Constructor for direct address. Not multilevel.
         */ 
        public LocatableEntity(int baseAddress, Memory pm){
            this.baseAddress = baseAddress;
            this.pm = pm;
        }

        /**
         * Returns the zPosition.
         */
        public float getZPos(){
           return this.zPos;
        }

        /*
         * Loads the location data from memory.
         */ 
        public bool loadLocationData()
        {
            //player data is 0x260 bytes long'ish
            byte[] buffer = new byte[0x10];
            pm.ReadMem(baseAddress, 0x10, out buffer);

            this.xPos = BitConverter.ToSingle(buffer, this.offsets.xPos);
            this.yPos = BitConverter.ToSingle(buffer, this.offsets.yPos);
            this.zPos = BitConverter.ToSingle(buffer, this.offsets.zPos);

            if (this.xPos > 0)
            {
                return true;
            }

            return false;
        }

        /*
         * Returns true if the entity is valid
         */ 
        public bool valid()
        {
            return this.xPos > 0;
        }

        /*
         * Calculates the closest entity to this entitiy from an array of other Locatable Entities.
         */ 
        public LocatableEntity GetClosestEntity(LocatableEntity[] entities)
        {
            LocatableEntity p = null;
            float distance = float.MaxValue;

            for (int i = 0; i < entities.Length; i++)
            {
                if (entities[i] != null)
                {
                    float td = Calculations.Get3dDistance(entities[i], this);
                    if (td < distance)
                    {
                        p = entities[i];
                        distance = td;
                    }
                }
            }
            return p;
        }


    }
}
