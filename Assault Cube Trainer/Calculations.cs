using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assault_Cube_Trainer
{

    /**
     *  Static class with some calculation functions. 
     */
    static class Calculations
    {

        public static float PI = (float)(Math.PI); //PI, used in calculations.

        /**
         * Converts an entities position to screen co-ordinates. Accepts the matrix(column major), PlayerEntity, Game Window Rect, and a boolean to specify if we are using the zPosFoot or zPosHead 
         * 
         * Mostly "borrowed" from 0XDE57
         * 
         * */
        public static float[] WorldToScreen(byte[] matrix, LocatableEntity entity, POINTS gameWindow, bool foot = true)
        {
            int width = gameWindow.Right - gameWindow.Left;  //calculate the width of the game window
            int height = gameWindow.Bottom - gameWindow.Top; //calculate the height of the game window
            

            /*
             *  Read the Matrix into variables for later use - it is column major
             */
            float m11 = BitConverter.ToSingle(matrix, 0), m12 = BitConverter.ToSingle(matrix, 4), m13 = BitConverter.ToSingle(matrix, 8), m14 = BitConverter.ToSingle(matrix, 12); 
            float m21 = BitConverter.ToSingle(matrix, 16), m22 = BitConverter.ToSingle(matrix, 20), m23 = BitConverter.ToSingle(matrix, 24), m24 = BitConverter.ToSingle(matrix, 28); 
            float m31 = BitConverter.ToSingle(matrix, 32), m32 = BitConverter.ToSingle(matrix, 36), m33 = BitConverter.ToSingle(matrix, 40), m34 = BitConverter.ToSingle(matrix, 44); 
            float m41 = BitConverter.ToSingle(matrix, 48), m42 = BitConverter.ToSingle(matrix, 52), m43 = BitConverter.ToSingle(matrix, 56), m44 = BitConverter.ToSingle(matrix, 60); 

            float zPos = entity is Player ? ((Player)entity).getZPos(foot) : entity.getZPos(); //get the zPos, foot is only relevant to player entities at this time

            //multiply vector against matrix
            float screenX = (m11 * entity.xPos) + (m21 * entity.yPos) + (m31 * zPos) + m41;
            float screenY = (m12 * entity.xPos) + (m22 * entity.yPos) + (m32 * zPos) + m42;
            float screenW = (m14 * entity.xPos) + (m24 * entity.yPos) + (m34 * zPos) + m44;


            //camera position (eye level/middle of screen)
            float camX = width / 2f;
            float camY = height / 2f;

            //convert to homogeneous position
            float x = camX + (camX * screenX / screenW);
            float y = camY - (camY * screenY / screenW);
            float[] screenPos = { x, y };

            //check it is in the bounds to draw
            if (screenW > 0.001f  //not behind us
                && gameWindow.Left + x > gameWindow.Left && gameWindow.Left + x < gameWindow.Right //not off the left or right of the window
                && gameWindow.Top + y > gameWindow.Top && gameWindow.Top + y < gameWindow.Bottom) //not off the top of bottom of the window
            {
                return screenPos;
            }
            return null;
        }

        public static float Get3dDistance(LocatableEntity to, LocatableEntity from, bool foot = false)
        {
            float zPosTo = to is Player ? ((Player)to).getZPos(foot) : to.getZPos(); //get the zPos, foot is only relevant to player entities at this time
            float zPosFrom = from is Player ? ((Player)from).getZPos(foot) : from.getZPos(); //get the zPos, foot is only relevant to player entities at this time


            return (float)(Math.Sqrt(
                ((to.xPos - from.xPos) * (to.xPos - from.xPos)) +
                ((to.yPos - from.yPos) * (to.yPos - from.yPos)) +
                ((zPosTo - zPosFrom) * (zPosTo - zPosFrom))
                ));
        }

        public static float[] EntityAimCoord(LocatableEntity to, LocatableEntity from,  bool foot = false){
            float zPosTo = to is Player ? ((Player)to).getZPos(foot) : to.getZPos(); //get the zPos, foot is only relevant to player entities at this time
            float zPosFrom = from is Player ? ((Player)from).getZPos(foot) : from.getZPos(); //get the zPos, foot is only relevant to player entities at this time


            //calculate Y
            float pitchY = (float)Math.Atan2(zPosTo - zPosFrom,
                    Get3dDistance(to, from))
                    * 180 / PI;

            //calculate X
            float yawX = -(float)Math.Atan2(to.xPos - from.xPos, to.yPos - from.yPos)
                / PI * 180 + 180;


            float[] xy = { yawX, pitchY };


            return xy;
        }

    }
}
