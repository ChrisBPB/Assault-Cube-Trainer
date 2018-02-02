using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
namespace Assault_Cube_Trainer
{
    public class GameManager
    {

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

        public int baseAddress;
        public Memory pm;

        public PlayerEntity[] players;
        public PlayerEntity localPlayer;

        public Dictionary<PlayerEntity, PlayerEntity[]> espEntities;

        public GameManager(int baseAddress, Memory pm)
        {
            this.baseAddress = baseAddress;
            this.pm = pm;
        }

        public void startPlayerThread()
        {
            //need a stop or pause option when I stop being lazy
            Thread thread = new Thread(new ThreadStart(loopPlayerLoad));
            thread.Start();
            thread.IsBackground = true;
        }

        public void loopPlayerLoad()
        {
            while (true)
            {
                loadNonLocalPlayers();
                loadLocalPlayer();
            }
        }

        public void loadLocalPlayer()
        {
            if (localPlayer == null)
            {
                localPlayer = new PlayerEntity(this.baseAddress + this.offsets.localPlayer, pm);
                espEntities = new Dictionary<PlayerEntity, PlayerEntity[]>();
                espEntities.Add(localPlayer, players);
            }
            
            localPlayer.loadPlayerData();
        }

        public void loadNonLocalPlayers()
        {
            int playerArray = pm.ReadInt(this.baseAddress + this.offsets.playerArrayOffset);
            int numberOfPlayers = pm.ReadInt(this.baseAddress + this.offsets.numberOfPlayersOffset);
            

            if (numberOfPlayers > 0)
            {
                if ( players == null || numberOfPlayers!=players.Count() || (players.Count()>0 && players[0] != null && players[0].xPos<0)) //check if object is invalid, if it is then we need to fetch new data
                {
                    localPlayer = null;
                    players = new PlayerEntity[numberOfPlayers];
                    uint size = (uint)(numberOfPlayers * 0x04);
                    byte[] buffer = new byte[size];
                    pm.ReadMem(playerArray, size, out buffer);

                    for (int i = 0; i < numberOfPlayers; i++)
                    {
                        int player = BitConverter.ToInt32(buffer, i * 0x04);
                        if (player > 0)
                        {
                            PlayerEntity p = new PlayerEntity(player, pm);
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

    }
}
