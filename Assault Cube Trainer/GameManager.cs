using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
namespace Assault_Cube_Trainer
{
    /*
     * Represents the game itself
     */ 
    public class GameManager
    {
        /**
         * Offsets for the Game
         */ 
        public struct Offsets
        {
            public int viewMatrix, playerArrayOffset, numberOfPlayersOffset, localPlayer;
 
            public Offsets(int viewMatrix, int playerArrayOffset, int numberOfPlayersOffset, int localPlayer){
                this.viewMatrix = viewMatrix;
                this.playerArrayOffset = playerArrayOffset;
                this.numberOfPlayersOffset = playerArrayOffset+numberOfPlayersOffset;
                this.localPlayer = localPlayer;
            }
        }

        public Offsets offsets = new Offsets(
                0x101AE8, 
                0x10F4F8, 
                0x08,
                0x10F4F4
            );

        public int baseAddress; //base address of the main game module
        public Memory pm;

        public Player[] players; //array of players
        public Player localPlayer; //our local player
        public byte[] viewMatrix; //our MvP matrix

        public Dictionary<LocatableEntity, LocatableEntity[]> espEntities; //Locatables we want ESP for

        public GameManager(int baseAddress, Memory pm)
        {
            this.baseAddress = baseAddress;
            this.pm = pm;
        }

        /**
         * Starts a thread to constantly scan players
         */ 
        public void startPlayerThread()
        {
            //need a stop or pause option when I stop being lazy
            Thread thread = new Thread(new ThreadStart(loopPlayerLoad));
            thread.Start();
            thread.IsBackground = true;
        }

        /**
         * Our looped thread method
         */ 
        public void loopPlayerLoad()
        {
            while (true)
            {
                loadViewMatrix();
                loadNonLocalPlayers();
                loadLocalPlayer();
            }
        }

        /**
         * Updates/loads the MVP matrix
         */ 
        public void loadViewMatrix(){
            this.viewMatrix = pm.ReadMatrix(this.baseAddress + this.offsets.viewMatrix);
        }

        /**
         * Loads the local player data
         */ 
        public void loadLocalPlayer()
        {
            if (localPlayer == null) //checks it we have already read it once. If we have then no point making another object..
            {
                localPlayer = new Player(this.baseAddress + this.offsets.localPlayer, new int[] { 0x0 }, pm);
                espEntities = new Dictionary<LocatableEntity, LocatableEntity[]>();
                espEntities.Add(localPlayer, players);
            }
            
            localPlayer.loadPlayerData();
        }

        /**
         * Loads all non local players
         */ 
        public void loadNonLocalPlayers()
        {
            int playerArray = pm.ReadInt(this.baseAddress + this.offsets.playerArrayOffset);
            int numberOfPlayers = pm.ReadInt(this.baseAddress + this.offsets.numberOfPlayersOffset);
            

            if (numberOfPlayers > 0) //check there are players ..
            {
                if ( players == null || numberOfPlayers!=players.Count() || (players.Count()>0 && players[0] != null && players[0].xPos<0)) //check if object is invalid, if it is then we need to fetch new data
                {
                    localPlayer = null;
                    players = new Player[numberOfPlayers];
                    uint size = (uint)(numberOfPlayers * 0x04); //get the address
                    byte[] buffer = new byte[size];
                    pm.ReadMem(playerArray, size, out buffer);

                    for (int i = 0; i < numberOfPlayers; i++)
                    {
                        int player = BitConverter.ToInt32(buffer, i * 0x04);
                        if (player > 0)
                        {
                            Player p = new Player(player, pm);
                            p.loadPlayerData(); //this returns true or false, maybe use as validation also
                            players[i] = p;

                        }
                    }
                }
                else
                {
                    for (int i = 0; i < numberOfPlayers; i++)
                    {
                        if (players[i] != null) {
                            players[i].loadPlayerData();
                        }
                    }
                }
            }

        }

        /**
         * Locks the local player to the closest enemy (as the eagle flys) 
         */ 
        public void lockLocalToClosest()
        {
            if (localPlayer != null && players != null && players.Count() > 0)
            {
                LocatableEntity p = localPlayer.GetClosestEntity(players);
                localPlayer.LockTarget(p);
            }
        }

        

    }
}
