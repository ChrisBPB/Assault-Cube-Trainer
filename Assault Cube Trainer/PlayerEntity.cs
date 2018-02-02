using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assault_Cube_Trainer
{
    public class PlayerEntity
    {

        public struct Offsets
        {
            public int health, xPos, yPos, zPosHead, zPosFoot, yaw, pitch, armour, ammoSpare, ammoClip, grenadeAmmo, fireRate, name;

            public Offsets(int health, int xPos, int yPos, int zPosHead, int zPosFoot, int yaw, int pitch, int armour, int ammoSpare, int ammoClip, int grenadeAmmo, int fireRate, int name)
            {
                this.health = health;
                this.xPos = xPos;
                this.yPos = yPos;
                this.zPosHead = zPosHead;
                this.zPosFoot = zPosFoot;
                this.yaw = yaw;
                this.pitch = pitch;
                this.armour = armour;
                this.ammoSpare = ammoSpare;
                this.ammoClip = ammoClip;
                this.grenadeAmmo = grenadeAmmo;
                this.name = name;
                this.fireRate = fireRate;

            }
        }

        public Offsets offsets = new Offsets(
                0xF8,
                0x04,
                0x08,
                0x0C,
                0x3C,
                0x40,
                0x44,
                0xFC,
                0x124,
                0x14C,
                0x158,
                0x174,
                0x225
            );

        public int baseAddress;

        public float xPos, yPos, zPosHead, zPosFoot, yaw, pitch;
        public int health, armour, ammoSpare, ammoClip, grenadeAmmo, rapidFire;
        public string name;

        private Memory pm;

        public PlayerEntity(int baseAddress, int[] multiLevel, Memory pm)
        {
            this.baseAddress = pm.ReadMultiLevelPointer(baseAddress, 4, multiLevel);
            this.pm = pm;
        }

        public PlayerEntity(int baseAddress, Memory pm){
            this.baseAddress = baseAddress;
            this.pm = pm;
        }

        public bool loadPlayerData()
        {
            //player data is 0x260 bytes long'ish
            byte[] buffer = new byte[0x260];
            pm.ReadMem(baseAddress, 0x260, out buffer);

            this.xPos = BitConverter.ToSingle(buffer, this.offsets.xPos);
            this.yPos = BitConverter.ToSingle(buffer, this.offsets.yPos);
            this.zPosHead = BitConverter.ToSingle(buffer, this.offsets.zPosHead);
            this.zPosFoot = BitConverter.ToSingle(buffer, this.offsets.zPosFoot);
            this.yaw = BitConverter.ToSingle(buffer, this.offsets.yaw);
            this.pitch = BitConverter.ToSingle(buffer, this.offsets.pitch);

            this.health = BitConverter.ToInt32(buffer, this.offsets.health);
            this.armour = BitConverter.ToInt32(buffer, this.offsets.health);
            this.ammoSpare = BitConverter.ToInt32(buffer, this.offsets.health);
            this.ammoClip = BitConverter.ToInt32(buffer, this.offsets.health);
            this.grenadeAmmo = BitConverter.ToInt32(buffer, this.offsets.health);
            this.rapidFire = BitConverter.ToInt32(buffer, this.offsets.health);

            this.name = BitConverter.ToString(buffer, this.offsets.name); //our offset for this is not correct

            if (this.xPos > 0)
            {
                return true;
            }

            return false;
        }

    }
}
