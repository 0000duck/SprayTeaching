// The RoboDK class allows interacting with RoboDK
// This library includes a robotics toolbox for C#, inspired from Peter Corke's Robotics Toolbox:
// http://petercorke.com/Robotics_Toolbox.html
//
// In this library: pose = transformation matrix = homogeneous matrix = 4x4 matrix
// Visit: http://www.j3d.org/matrix_faq/matrfaq_latest.html
// to better understand homogeneous matrix operations
//
// This library includes the mathematics to operate with homogeneous matrices for robotics.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;       // for Socket
//using System.Text.RegularExpressions;

/// <summary>
/// Matrix class for robotics. 
/// </summary>
public class Mat // simple matrix class for homogeneous operations
{
    public int rows;
    public int cols;
    public double[,] mat;

    public Mat L;
    public Mat U;

    //  Class used for Matrix exceptions
    public class MatException : Exception
    {
        public MatException(string Message)
            : base(Message)
        { }
    }

    /// <summary>
    /// Matrix class constructor for any size matrix
    /// </summary>
    /// <param name="Rows">dimension 1 size (rows)</param>
    /// <param name="Cols">dimension 2 size (columns)</param>
    public Mat(int Rows, int Cols)         // Matrix Class constructor
    {
        rows = Rows;
        cols = Cols;
        mat = new double[rows, cols];
    }

    /// <summary>
    /// Matrix class constructor for a 4x4 homogeneous matrix
    /// </summary>
    /// <param name="nx">Position [0,0]</param>
    /// <param name="ox">Position [0,1]</param>
    /// <param name="ax">Position [0,2]</param>
    /// <param name="tx">Position [0,3]</param>
    /// <param name="ny">Position [1,0]</param>
    /// <param name="oy">Position [1,1]</param>
    /// <param name="ay">Position [1,2]</param>
    /// <param name="ty">Position [1,3]</param>
    /// <param name="nz">Position [2,0]</param>
    /// <param name="oz">Position [2,1]</param>
    /// <param name="az">Position [2,2]</param>
    /// <param name="tz">Position [2,3]</param>
    public Mat(double nx, double ox, double ax, double tx, double ny, double oy, double ay, double ty, double nz, double oz, double az, double tz)         // Matrix Class constructor
    {
        rows = 4;
        cols = 4;
        mat = new double[rows, cols];
        mat[0, 0] = nx; mat[1, 0] = ny; mat[2, 0] = nz;
        mat[0, 1] = ox; mat[1, 1] = oy; mat[2, 1] = oz;
        mat[0, 2] = ax; mat[1, 2] = ay; mat[2, 2] = az;
        mat[0, 3] = tx; mat[1, 3] = ty; mat[2, 3] = tz;
        mat[3, 0] = 0.0; mat[3, 1] = 0.0; mat[3, 2] = 0.0; mat[3, 3] = 1.0;
    }

    /// <summary>
    /// Matrix class constructor for a 4x1 vector [x,y,z,1]
    /// </summary>
    /// <param name="x">x coordinate</param>
    /// <param name="y">y coordinate</param>
    /// <param name="z">z coordinate</param>
    public Mat(double x, double y, double z)
    {
        rows = 4;
        cols = 1;
        mat = new double[rows, cols];
        mat[0, 0] = x;
        mat[1, 0] = y;
        mat[2, 0] = z;
        mat[3, 0] = 1.0;
    }

    //----------------------------------------------------
    //--------     Generic matrix usage    ---------------
    /// <summary>
    /// Return a translation matrix
    ///                 |  1   0   0   X |
    /// transl(X,Y,Z) = |  0   1   0   Y |
    ///                 |  0   0   1   Z |
    ///                 |  0   0   0   1 |
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <returns></returns>
    static public Mat transl(double x, double y, double z)
    {
        Mat mat = Mat.IdentityMatrix(4, 4);
        mat.setPos(x, y, z);
        return mat;
    }

    /// <summary>
    /// Return a X-axis rotation matrix
    ///            |  1  0        0        0 |
    /// rotx(rx) = |  0  cos(rx) -sin(rx)  0 |
    ///            |  0  sin(rx)  cos(rx)  0 |
    ///            |  0  0        0        1 |
    /// </summary>
    /// <param name="rx">rotation around X axis (in radians)</param>
    /// <returns></returns>
    static public Mat rotx(double rx)
    {
        double cx = Math.Cos(rx);
        double sx = Math.Sin(rx);
        return new Mat(1, 0, 0, 0, 0, cx, -sx, 0, 0, sx, cx, 0);
    }

    /// <summary>
    /// Return a Y-axis rotation matrix
    ///            |  cos(ry)  0   sin(ry)  0 |
    /// roty(ry) = |  0        1   0        0 |
    ///            | -sin(ry)  0   cos(ry)  0 |
    ///            |  0        0   0        1 |
    /// </summary>
    /// <param name="ry">rotation around Y axis (in radians)</param>
    /// <returns></returns>
    static public Mat roty(double ry)
    {
        double cy = Math.Cos(ry);
        double sy = Math.Sin(ry);
        return new Mat(cy, 0, sy, 0, 0, 1, 0, 0, -sy, 0, cy, 0);
    }

    /// <summary>
    /// Return a Z-axis rotation matrix
    ///            |  cos(rz)  -sin(rz)   0   0 |
    /// rotz(rx) = |  sin(rz)   cos(rz)   0   0 |
    ///            |  0         0         1   0 |
    ///            |  0         0         0   1 |
    /// </summary>
    /// <param name="rz"></param>
    /// <returns></returns>
    static public Mat rotz(double rz)
    {
        double cz = Math.Cos(rz);
        double sz = Math.Sin(rz);
        return new Mat(cz, -sz, 0, 0, sz, cz, 0, 0, 0, 0, 1, 0);
    }


    //----------------------------------------------------
    //------ Pose to xyzrpw and xyzrpw to pose------------
    /// <summary>
    /// Calculates the equivalent position and euler angles ([x,y,z,r,p,r] vector) of the given pose 
    /// Note: transl(x,y,z)*rotz(r*pi/180)*roty(p*pi/180)*rotx(r*pi/180)
    /// See also: FromXYZRPW()
    /// </summary>
    /// <returns>XYZWPR translation and rotation in mm and degrees</returns>
    public double[] ToXYZRPW( )
    {
        double[] xyzwpr = new double[6];
        double x = mat[0, 3];
        double y = mat[1, 3];
        double z = mat[2, 3];
        double w, p, r;
        if (mat[2, 0] > (1.0 - 1e-6))
        {
            p = -Math.PI * 0.5;
            r = 0;
            w = Math.Atan2(-mat[1, 2], mat[1, 1]);
        }
        else if (mat[2, 0] < -1.0 + 1e-6)
        {
            p = 0.5 * Math.PI;
            r = 0;
            w = Math.Atan2(mat[1, 2], mat[1, 1]);
        }
        else
        {
            p = Math.Atan2(-mat[2, 0], Math.Sqrt(mat[0, 0] * mat[0, 0] + mat[1, 0] * mat[1, 0]));
            w = Math.Atan2(mat[1, 0], mat[0, 0]);
            r = Math.Atan2(mat[2, 1], mat[2, 2]);
        }
        xyzwpr[0] = x;
        xyzwpr[1] = y;
        xyzwpr[2] = z;
        xyzwpr[3] = r * 180.0 / Math.PI;
        xyzwpr[4] = p * 180.0 / Math.PI;
        xyzwpr[5] = w * 180.0 / Math.PI;
        return xyzwpr;
    }

    /// <summary>
    /// Calculates the pose from the position and euler angles ([x,y,z,r,p,r] vector)
    /// The result is the same as calling: H = transl(x,y,z)*rotz(r*pi/180)*roty(p*pi/180)*rotx(r*pi/180)
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <param name="r"></param>
    /// <param name="p"></param>
    /// <param name="r"></param>
    /// <returns>Homogeneous matrix (4x4)</returns>
    static public Mat FromXYZRPW(double x, double y, double z, double w, double p, double r)
    {
        double a = r * Math.PI / 180.0;
        double b = p * Math.PI / 180.0;
        double c = w * Math.PI / 180.0;
        double ca = Math.Cos(a);
        double sa = Math.Sin(a);
        double cb = Math.Cos(b);
        double sb = Math.Sin(b);
        double cc = Math.Cos(c);
        double sc = Math.Sin(c);
        return new Mat(cb * cc, cc * sa * sb - ca * sc, sa * sc + ca * cc * sb, x, cb * sc, ca * cc + sa * sb * sc, ca * sb * sc - cc * sa, y, -sb, cb * sa, ca * cb, z);
    }

    /// <summary>
    /// Returns the quaternion of a pose (4x4 matrix)
    /// </summary>
    /// <param name="Ti"></param>
    /// <returns></returns>
    static double[] ToQuaternion(Mat Ti)
    {
        double[] q = new double[4];
        double a = (Ti[0, 0]);
        double b = (Ti[1, 1]);
        double c = (Ti[2, 2]);
        double sign2 = 1.0;
        double sign3 = 1.0;
        double sign4 = 1.0;
        if ((Ti[2, 1] - Ti[1, 2]) < 0)
        {
            sign2 = -1;
        }
        if ((Ti[0, 2] - Ti[2, 0]) < 0)
        {
            sign3 = -1;
        }
        if ((Ti[1, 0] - Ti[0, 1]) < 0)
        {
            sign4 = -1;
        }
        q[0] = 0.5 * Math.Sqrt(Math.Max(a + b + c + 1, 0));
        q[1] = 0.5 * sign2 * Math.Sqrt(Math.Max(a - b - c + 1, 0));
        q[2] = 0.5 * sign3 * Math.Sqrt(Math.Max(-a + b - c + 1, 0));
        q[3] = 0.5 * sign4 * Math.Sqrt(Math.Max(-a - b + c + 1, 0));
        return q;
    }

    /// <summary>
    /// Returns the pose (4x4 matrix) from quaternion data
    /// </summary>
    /// <param name="q"></param>
    /// <returns></returns>
    static Mat FromQuaternion(double[] qin)
    {
        double qnorm = Math.Sqrt(qin[0] * qin[0] + qin[1] * qin[1] + qin[2] * qin[2] + qin[3] * qin[3]);
        double[] q = new double[4];
        q[0] = qin[0] / qnorm;
        q[1] = qin[1] / qnorm;
        q[2] = qin[2] / qnorm;
        q[3] = qin[3] / qnorm;
        Mat pose = new Mat(1 - 2 * q[2] * q[2] - 2 * q[3] * q[3], 2 * q[1] * q[2] - 2 * q[3] * q[0], 2 * q[1] * q[3] + 2 * q[2] * q[0], 0, 2 * q[1] * q[2] + 2 * q[3] * q[0], 1 - 2 * q[1] * q[1] - 2 * q[3] * q[3], 2 * q[2] * q[3] - 2 * q[1] * q[0], 0, 2 * q[1] * q[3] - 2 * q[2] * q[0], 2 * q[2] * q[3] + 2 * q[1] * q[0], 1 - 2 * q[1] * q[1] - 2 * q[2] * q[2], 0);
        return pose;
    }

    /// <summary>
    /// Converts a pose to an ABB target
    /// </summary>
    /// <param name="H"></param>
    /// <returns></returns>
    static double[] ToABB(Mat H)
    {
        double[] q = ToQuaternion(H);
        double[] xyzq1234 = { H[0, 3], H[1, 3], H[2, 3], q[0], q[1], q[2], q[3] };
        return xyzq1234;
    }


    /// <summary>
    /// Calculates the pose from the position and euler angles ([x,y,z,r,p,r] vector)
    //  The result is the same as calling: H = transl(x,y,z)*rotz(r*pi/180)*roty(p*pi/180)*rotx(r*pi/180)
    /// </summary>
    /// <param name="xyzwpr"></param>
    /// <returns>Homogeneous matrix (4x4)</returns>
    static public Mat FromXYZRPW(double[] xyzwpr)
    {
        if (xyzwpr.Length < 6)
        {
            return null;
        }
        return FromXYZRPW(xyzwpr[0], xyzwpr[1], xyzwpr[2], xyzwpr[3], xyzwpr[4], xyzwpr[5]);
    }

    /// <summary>
    /// Calculates the equivalent position and euler angles ([x,y,z,r,p,r] vector) of the given pose in Universal Robots format
    /// Note: The difference between ToUR and ToXYZWPR is that the first one uses radians for the orientation and the second one uses degres
    /// Note: transl(x,y,z)*rotx(rx*pi/180)*roty(ry*pi/180)*rotz(rz*pi/180)
    /// See also: FromXYZRPW()
    /// </summary>
    /// <returns>XYZWPR translation and rotation in mm and radians</returns>
    public double[] ToUR( )
    {
        double[] xyzwpr = new double[6];
        double x = mat[0, 3];
        double y = mat[1, 3];
        double z = mat[2, 3];
        double angle = Math.Acos(Math.Min(Math.Max((mat[0, 0] + mat[1, 1] + mat[2, 2] - 1) / 2, -1), 1));
        double rx = mat[2, 1] - mat[1, 2];
        double ry = mat[0, 2] - mat[2, 0];
        double rz = mat[1, 0] - mat[0, 1];
        if (angle == 0)
        {
            rx = 0;
            ry = 0;
            rz = 0;
        }
        else
        {
            rx = rx * angle / (2 * Math.Sin(angle));
            ry = ry * angle / (2 * Math.Sin(angle));
            rz = rz * angle / (2 * Math.Sin(angle));
        }
        xyzwpr[0] = x;
        xyzwpr[1] = y;
        xyzwpr[2] = z;
        xyzwpr[3] = rx;
        xyzwpr[4] = ry;
        xyzwpr[5] = rz;
        return xyzwpr;
    }

    /// <summary>
    /// Calculates the pose from the position and euler angles ([x,y,z,r,p,r] vector)
    /// Note: The difference between FromUR and FromXYZWPR is that the first one uses radians for the orientation and the second one uses degres
    /// The result is the same as calling: H = transl(x,y,z)*rotx(rx)*roty(ry)*rotz(rz)
    /// </summary>
    /// <param name="xyzwpr">The position and euler angles array</param>
    /// <returns>Homogeneous matrix (4x4)</returns>
    public static Mat FromUR(double[] xyzwpr)
    {
        double x = xyzwpr[0];
        double y = xyzwpr[1];
        double z = xyzwpr[2];
        double w = xyzwpr[3];
        double p = xyzwpr[4];
        double r = xyzwpr[5];
        double angle = Math.Sqrt(w * w + p * p + r * r);
        if (angle < 1e-6)
        {
            return Identity4x4();
        }
        double c = Math.Cos(angle);
        double s = Math.Sin(angle);
        double ux = w / angle;
        double uy = p / angle;
        double uz = r / angle;
        return new Mat(ux * ux + c * (1 - ux * ux), ux * uy * (1 - c) - uz * s, ux * uz * (1 - c) + uy * s, x, ux * uy * (1 - c) + uz * s, uy * uy + (1 - uy * uy) * c, uy * uz * (1 - c) - ux * s, y, ux * uz * (1 - c) - uy * s, uy * uz * (1 - c) + ux * s, uz * uz + (1 - uz * uz) * c, z);
    }

    /// <summary>
    /// Converts a matrix into a one-dimensional array of doubles
    /// </summary>
    /// <returns>one-dimensional array</returns>
    public double[] ToDoubles( )
    {
        int cnt = 0;
        double[] array = new double[rows * cols];
        for (int j = 0; j < cols; j++)
        {
            for (int i = 0; i < rows; i++)
            {
                array[cnt] = mat[i, j];
                cnt = cnt + 1;
            }
        }
        return array;
    }

    /// <summary>
    /// Check if the matrix is square
    /// </summary>
    public Boolean IsSquare( )
    {
        return (rows == cols);
    }

    public Boolean Is4x4( )
    {
        if (cols != 4 || rows != 4)
        {
            return false;
        }
        return true;
    }

    /// <summary>
    /// Check if the matrix is homogeneous (4x4)
    /// </summary>
    public Boolean IsHomogeneous( )
    {
        if (!Is4x4())
        {
            return false;
        }
        return true;
        /*
        test = self[0:3,0:3];
        test = test*test.tr()
        test[0,0] = test[0,0] - 1.0
        test[1,1] = test[1,1] - 1.0
        test[2,2] = test[2,2] - 1.0
        zero = 0.0
        for x in range(3):
            for y in range(3):
                zero = zero + abs(test[x,y])
        if zero > 1e-4:
            return False
        return True
        */
    }

    /// <summary>
    /// Returns the inverse of a homogeneous matrix (4x4 matrix)
    /// </summary>
    /// <returns>Homogeneous matrix (4x4)</returns>
    public Mat inv( )
    {
        if (!IsHomogeneous())
        {
            throw new MatException("Can't invert a non-homogeneous matrix");
        }
        double[] xyz = this.Pos();
        Mat mat_xyz = new Mat(xyz[0], xyz[1], xyz[2]);
        Mat hinv = this.Duplicate();
        hinv.setPos(0, 0, 0);
        hinv = hinv.Transpose();
        Mat new_pos = rotate(hinv, mat_xyz);
        hinv[0, 3] = -new_pos[0, 0];
        hinv[1, 3] = -new_pos[1, 0];
        hinv[2, 3] = -new_pos[2, 0];
        return hinv;
    }

    /// <summary>
    /// Rotate a vector given a matrix (rotation matrix or homogeneous matrix)
    /// </summary>
    /// <param name="pose">4x4 homogeneous matrix or 3x3 rotation matrix</param>
    /// <param name="vector">4x1 or 3x1 vector</param>
    /// <returns></returns>
    static public Mat rotate(Mat pose, Mat vector)
    {
        if (pose.cols < 3 || pose.rows < 3 || vector.rows < 3)
        {
            throw new MatException("Invalid matrix size");
        }
        Mat pose3x3 = pose.Duplicate();
        Mat vector3 = vector.Duplicate();
        pose3x3.rows = 3;
        pose3x3.cols = 3;
        vector3.rows = 3;
        return pose3x3 * vector3;
    }

    /// <summary>
    /// Returns the XYZ position of the Homogeneous matrix
    /// </summary>
    /// <returns>XYZ position</returns>
    public double[] Pos( )
    {
        if (!Is4x4())
        {
            return null;
        }
        double[] xyz = new double[3];
        xyz[0] = mat[0, 3]; xyz[1] = mat[1, 3]; xyz[2] = mat[2, 3];
        return xyz;
    }

    /// <summary>
    /// Sets the 4x4 position of the Homogeneous matrix
    /// </summary>
    /// <param name="xyz">XYZ position</param>
    public void setPos(double[] xyz)
    {
        if (!Is4x4() || xyz.Length < 3)
        {
            return;
        }
        mat[0, 3] = xyz[0]; mat[1, 3] = xyz[1]; mat[2, 3] = xyz[2];
    }

    /// <summary>
    /// Sets the 4x4 position of the Homogeneous matrix
    /// </summary>
    /// <param name="x">X position</param>
    /// <param name="y">Y position</param>
    /// <param name="z">Z position</param>
    public void setPos(double x, double y, double z)
    {
        if (!Is4x4())
        {
            return;
        }
        mat[0, 3] = x; mat[1, 3] = y; mat[2, 3] = z;
    }


    public double this[int iRow, int iCol]      // Access this matrix as a 2D array
    {
        get { return mat[iRow, iCol]; }
        set { mat[iRow, iCol] = value; }
    }

    public Mat GetCol(int k)
    {
        Mat m = new Mat(rows, 1);
        for (int i = 0; i < rows; i++) m[i, 0] = mat[i, k];
        return m;
    }

    public void SetCol(Mat v, int k)
    {
        for (int i = 0; i < rows; i++) mat[i, k] = v[i, 0];
    }

    public Mat Duplicate( )                   // Function returns the copy of this matrix
    {
        Mat matrix = new Mat(rows, cols);
        for (int i = 0; i < rows; i++)
            for (int j = 0; j < cols; j++)
                matrix[i, j] = mat[i, j];
        return matrix;
    }

    public static Mat ZeroMatrix(int iRows, int iCols)       // Function generates the zero matrix
    {
        Mat matrix = new Mat(iRows, iCols);
        for (int i = 0; i < iRows; i++)
            for (int j = 0; j < iCols; j++)
                matrix[i, j] = 0;
        return matrix;
    }

    public static Mat IdentityMatrix(int iRows, int iCols)   // Function generates the identity matrix
    {
        Mat matrix = ZeroMatrix(iRows, iCols);
        for (int i = 0; i < Math.Min(iRows, iCols); i++)
            matrix[i, i] = 1;
        return matrix;
    }

    /// <summary>
    /// Returns an identity 4x4 matrix (homogeneous matrix)
    /// </summary>
    /// <returns></returns>
    public static Mat Identity4x4( )
    {
        return Mat.IdentityMatrix(4, 4);
    }

    /*
    public static Mat Parse(string ps)                        // Function parses the matrix from string
    {
        string s = NormalizeMatrixString(ps);
        string[] rows = Regex.Split(s, "\r\n");
        string[] nums = rows[0].Split(' ');
        Mat matrix = new Mat(rows.Length, nums.Length);
        try
        {
            for (int i = 0; i < rows.Length; i++)
            {
                nums = rows[i].Split(' ');
                for (int j = 0; j < nums.Length; j++) matrix[i, j] = double.Parse(nums[j]);
            }
        }
        catch (FormatException exc) { throw new MatException("Wrong input format!"); }
        return matrix;
    }*/

    public override string ToString( )                           // Function returns matrix as a string
    {
        string s = "";
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++) s += String.Format("{0,5:0.00}", mat[i, j]) + " ";
            s += "\r\n";
        }
        return s;
    }

    /// <summary>
    /// Transpose a matrix
    /// </summary>
    /// <returns></returns>
    public Mat Transpose( )
    {
        return Transpose(this);
    }
    public static Mat Transpose(Mat m)              // Matrix transpose, for any rectangular matrix
    {
        Mat t = new Mat(m.cols, m.rows);
        for (int i = 0; i < m.rows; i++)
            for (int j = 0; j < m.cols; j++)
                t[j, i] = m[i, j];
        return t;
    }

    private static void SafeAplusBintoC(Mat A, int xa, int ya, Mat B, int xb, int yb, Mat C, int size)
    {
        for (int i = 0; i < size; i++)          // rows
            for (int j = 0; j < size; j++)     // cols
            {
                C[i, j] = 0;
                if (xa + j < A.cols && ya + i < A.rows) C[i, j] += A[ya + i, xa + j];
                if (xb + j < B.cols && yb + i < B.rows) C[i, j] += B[yb + i, xb + j];
            }
    }

    private static void SafeAminusBintoC(Mat A, int xa, int ya, Mat B, int xb, int yb, Mat C, int size)
    {
        for (int i = 0; i < size; i++)          // rows
            for (int j = 0; j < size; j++)     // cols
            {
                C[i, j] = 0;
                if (xa + j < A.cols && ya + i < A.rows) C[i, j] += A[ya + i, xa + j];
                if (xb + j < B.cols && yb + i < B.rows) C[i, j] -= B[yb + i, xb + j];
            }
    }

    private static void SafeACopytoC(Mat A, int xa, int ya, Mat C, int size)
    {
        for (int i = 0; i < size; i++)          // rows
            for (int j = 0; j < size; j++)     // cols
            {
                C[i, j] = 0;
                if (xa + j < A.cols && ya + i < A.rows) C[i, j] += A[ya + i, xa + j];
            }
    }

    private static void AplusBintoC(Mat A, int xa, int ya, Mat B, int xb, int yb, Mat C, int size)
    {
        for (int i = 0; i < size; i++)          // rows
            for (int j = 0; j < size; j++) C[i, j] = A[ya + i, xa + j] + B[yb + i, xb + j];
    }

    private static void AminusBintoC(Mat A, int xa, int ya, Mat B, int xb, int yb, Mat C, int size)
    {
        for (int i = 0; i < size; i++)          // rows
            for (int j = 0; j < size; j++) C[i, j] = A[ya + i, xa + j] - B[yb + i, xb + j];
    }

    private static void ACopytoC(Mat A, int xa, int ya, Mat C, int size)
    {
        for (int i = 0; i < size; i++)          // rows
            for (int j = 0; j < size; j++) C[i, j] = A[ya + i, xa + j];
    }

    private static Mat StrassenMultiply(Mat A, Mat B)                // Smart matrix multiplication
    {
        if (A.cols != B.rows) throw new MatException("Wrong dimension of matrix!");

        Mat R;

        int msize = Math.Max(Math.Max(A.rows, A.cols), Math.Max(B.rows, B.cols));

        if (msize < 32)
        {
            R = ZeroMatrix(A.rows, B.cols);
            for (int i = 0; i < R.rows; i++)
                for (int j = 0; j < R.cols; j++)
                    for (int k = 0; k < A.cols; k++)
                        R[i, j] += A[i, k] * B[k, j];
            return R;
        }

        int size = 1; int n = 0;
        while (msize > size) { size *= 2; n++; };
        int h = size / 2;


        Mat[,] mField = new Mat[n, 9];

        /*
         *  8x8, 8x8, 8x8, ...
         *  4x4, 4x4, 4x4, ...
         *  2x2, 2x2, 2x2, ...
         *  . . .
         */

        int z;
        for (int i = 0; i < n - 4; i++)          // rows
        {
            z = (int)Math.Pow(2, n - i - 1);
            for (int j = 0; j < 9; j++) mField[i, j] = new Mat(z, z);
        }

        SafeAplusBintoC(A, 0, 0, A, h, h, mField[0, 0], h);
        SafeAplusBintoC(B, 0, 0, B, h, h, mField[0, 1], h);
        StrassenMultiplyRun(mField[0, 0], mField[0, 1], mField[0, 1 + 1], 1, mField); // (A11 + A22) * (B11 + B22);

        SafeAplusBintoC(A, 0, h, A, h, h, mField[0, 0], h);
        SafeACopytoC(B, 0, 0, mField[0, 1], h);
        StrassenMultiplyRun(mField[0, 0], mField[0, 1], mField[0, 1 + 2], 1, mField); // (A21 + A22) * B11;

        SafeACopytoC(A, 0, 0, mField[0, 0], h);
        SafeAminusBintoC(B, h, 0, B, h, h, mField[0, 1], h);
        StrassenMultiplyRun(mField[0, 0], mField[0, 1], mField[0, 1 + 3], 1, mField); //A11 * (B12 - B22);

        SafeACopytoC(A, h, h, mField[0, 0], h);
        SafeAminusBintoC(B, 0, h, B, 0, 0, mField[0, 1], h);
        StrassenMultiplyRun(mField[0, 0], mField[0, 1], mField[0, 1 + 4], 1, mField); //A22 * (B21 - B11);

        SafeAplusBintoC(A, 0, 0, A, h, 0, mField[0, 0], h);
        SafeACopytoC(B, h, h, mField[0, 1], h);
        StrassenMultiplyRun(mField[0, 0], mField[0, 1], mField[0, 1 + 5], 1, mField); //(A11 + A12) * B22;

        SafeAminusBintoC(A, 0, h, A, 0, 0, mField[0, 0], h);
        SafeAplusBintoC(B, 0, 0, B, h, 0, mField[0, 1], h);
        StrassenMultiplyRun(mField[0, 0], mField[0, 1], mField[0, 1 + 6], 1, mField); //(A21 - A11) * (B11 + B12);

        SafeAminusBintoC(A, h, 0, A, h, h, mField[0, 0], h);
        SafeAplusBintoC(B, 0, h, B, h, h, mField[0, 1], h);
        StrassenMultiplyRun(mField[0, 0], mField[0, 1], mField[0, 1 + 7], 1, mField); // (A12 - A22) * (B21 + B22);

        R = new Mat(A.rows, B.cols);                  // result

        /// C11
        for (int i = 0; i < Math.Min(h, R.rows); i++)          // rows
            for (int j = 0; j < Math.Min(h, R.cols); j++)     // cols
                R[i, j] = mField[0, 1 + 1][i, j] + mField[0, 1 + 4][i, j] - mField[0, 1 + 5][i, j] + mField[0, 1 + 7][i, j];

        /// C12
        for (int i = 0; i < Math.Min(h, R.rows); i++)          // rows
            for (int j = h; j < Math.Min(2 * h, R.cols); j++)     // cols
                R[i, j] = mField[0, 1 + 3][i, j - h] + mField[0, 1 + 5][i, j - h];

        /// C21
        for (int i = h; i < Math.Min(2 * h, R.rows); i++)          // rows
            for (int j = 0; j < Math.Min(h, R.cols); j++)     // cols
                R[i, j] = mField[0, 1 + 2][i - h, j] + mField[0, 1 + 4][i - h, j];

        /// C22
        for (int i = h; i < Math.Min(2 * h, R.rows); i++)          // rows
            for (int j = h; j < Math.Min(2 * h, R.cols); j++)     // cols
                R[i, j] = mField[0, 1 + 1][i - h, j - h] - mField[0, 1 + 2][i - h, j - h] + mField[0, 1 + 3][i - h, j - h] + mField[0, 1 + 6][i - h, j - h];

        return R;
    }

    // function for square matrix 2^N x 2^N

    private static void StrassenMultiplyRun(Mat A, Mat B, Mat C, int l, Mat[,] f)    // A * B into C, level of recursion, matrix field
    {
        int size = A.rows;
        int h = size / 2;

        if (size < 32)
        {
            for (int i = 0; i < C.rows; i++)
                for (int j = 0; j < C.cols; j++)
                {
                    C[i, j] = 0;
                    for (int k = 0; k < A.cols; k++) C[i, j] += A[i, k] * B[k, j];
                }
            return;
        }

        AplusBintoC(A, 0, 0, A, h, h, f[l, 0], h);
        AplusBintoC(B, 0, 0, B, h, h, f[l, 1], h);
        StrassenMultiplyRun(f[l, 0], f[l, 1], f[l, 1 + 1], l + 1, f); // (A11 + A22) * (B11 + B22);

        AplusBintoC(A, 0, h, A, h, h, f[l, 0], h);
        ACopytoC(B, 0, 0, f[l, 1], h);
        StrassenMultiplyRun(f[l, 0], f[l, 1], f[l, 1 + 2], l + 1, f); // (A21 + A22) * B11;

        ACopytoC(A, 0, 0, f[l, 0], h);
        AminusBintoC(B, h, 0, B, h, h, f[l, 1], h);
        StrassenMultiplyRun(f[l, 0], f[l, 1], f[l, 1 + 3], l + 1, f); //A11 * (B12 - B22);

        ACopytoC(A, h, h, f[l, 0], h);
        AminusBintoC(B, 0, h, B, 0, 0, f[l, 1], h);
        StrassenMultiplyRun(f[l, 0], f[l, 1], f[l, 1 + 4], l + 1, f); //A22 * (B21 - B11);

        AplusBintoC(A, 0, 0, A, h, 0, f[l, 0], h);
        ACopytoC(B, h, h, f[l, 1], h);
        StrassenMultiplyRun(f[l, 0], f[l, 1], f[l, 1 + 5], l + 1, f); //(A11 + A12) * B22;

        AminusBintoC(A, 0, h, A, 0, 0, f[l, 0], h);
        AplusBintoC(B, 0, 0, B, h, 0, f[l, 1], h);
        StrassenMultiplyRun(f[l, 0], f[l, 1], f[l, 1 + 6], l + 1, f); //(A21 - A11) * (B11 + B12);

        AminusBintoC(A, h, 0, A, h, h, f[l, 0], h);
        AplusBintoC(B, 0, h, B, h, h, f[l, 1], h);
        StrassenMultiplyRun(f[l, 0], f[l, 1], f[l, 1 + 7], l + 1, f); // (A12 - A22) * (B21 + B22);

        /// C11
        for (int i = 0; i < h; i++)          // rows
            for (int j = 0; j < h; j++)     // cols
                C[i, j] = f[l, 1 + 1][i, j] + f[l, 1 + 4][i, j] - f[l, 1 + 5][i, j] + f[l, 1 + 7][i, j];

        /// C12
        for (int i = 0; i < h; i++)          // rows
            for (int j = h; j < size; j++)     // cols
                C[i, j] = f[l, 1 + 3][i, j - h] + f[l, 1 + 5][i, j - h];

        /// C21
        for (int i = h; i < size; i++)          // rows
            for (int j = 0; j < h; j++)     // cols
                C[i, j] = f[l, 1 + 2][i - h, j] + f[l, 1 + 4][i - h, j];

        /// C22
        for (int i = h; i < size; i++)          // rows
            for (int j = h; j < size; j++)     // cols
                C[i, j] = f[l, 1 + 1][i - h, j - h] - f[l, 1 + 2][i - h, j - h] + f[l, 1 + 3][i - h, j - h] + f[l, 1 + 6][i - h, j - h];
    }

    public static Mat StupidMultiply(Mat m1, Mat m2)                  // Stupid matrix multiplication
    {
        if (m1.cols != m2.rows) throw new MatException("Wrong dimensions of matrix!");

        Mat result = ZeroMatrix(m1.rows, m2.cols);
        for (int i = 0; i < result.rows; i++)
            for (int j = 0; j < result.cols; j++)
                for (int k = 0; k < m1.cols; k++)
                    result[i, j] += m1[i, k] * m2[k, j];
        return result;
    }
    private static Mat Multiply(double n, Mat m)                          // Multiplication by constant n
    {
        Mat r = new Mat(m.rows, m.cols);
        for (int i = 0; i < m.rows; i++)
            for (int j = 0; j < m.cols; j++)
                r[i, j] = m[i, j] * n;
        return r;
    }
    private static Mat Add(Mat m1, Mat m2)         // Add matrix
    {
        if (m1.rows != m2.rows || m1.cols != m2.cols) throw new MatException("Matrices must have the same dimensions!");
        Mat r = new Mat(m1.rows, m1.cols);
        for (int i = 0; i < r.rows; i++)
            for (int j = 0; j < r.cols; j++)
                r[i, j] = m1[i, j] + m2[i, j];
        return r;
    }

    public static string NormalizeMatrixString(string matStr)	// From Andy - thank you! :)
    {
        // Remove any multiple spaces
        while (matStr.IndexOf("  ") != -1)
            matStr = matStr.Replace("  ", " ");

        // Remove any spaces before or after newlines
        matStr = matStr.Replace(" \r\n", "\r\n");
        matStr = matStr.Replace("\r\n ", "\r\n");

        // If the data ends in a newline, remove the trailing newline.
        // Make it easier by first replacing \r\n’s with |’s then
        // restore the |’s with \r\n’s
        matStr = matStr.Replace("\r\n", "|");
        while (matStr.LastIndexOf("|") == (matStr.Length - 1))
            matStr = matStr.Substring(0, matStr.Length - 1);

        matStr = matStr.Replace("|", "\r\n");
        return matStr.Trim();
    }


    // Operators
    public static Mat operator -(Mat m)
    { return Mat.Multiply(-1, m); }

    public static Mat operator +(Mat m1, Mat m2)
    { return Mat.Add(m1, m2); }

    public static Mat operator -(Mat m1, Mat m2)
    { return Mat.Add(m1, -m2); }

    public static Mat operator *(Mat m1, Mat m2)
    { return Mat.StrassenMultiply(m1, m2); }

    public static Mat operator *(double n, Mat m)
    { return Mat.Multiply(n, m); }
}


/// <summary>
/// This class is the link to allows to create macros and automate Robodk.
/// Any interaction is made through \"items\" (Item() objects). An item is an object in the
/// robodk tree (it can be either a robot, an object, a tool, a frame, a program, ...).
/// </summary>
public class RoboDK
{
    /// <summary>
    /// Class used for RoboDK exceptions
    /// </summary>  
    public class RDKException : Exception
    {
        public RDKException(string Message)
            : base(Message)
        { }
    }

    // Tree item types
    public const int ITEM_TYPE_ANY = -1;
    public const int ITEM_TYPE_STATION = 1;
    public const int ITEM_TYPE_ROBOT = 2;
    public const int ITEM_TYPE_FRAME = 3;
    public const int ITEM_TYPE_TOOL = 4;
    public const int ITEM_TYPE_OBJECT = 5;
    public const int ITEM_TYPE_TARGET = 6;
    public const int ITEM_TYPE_PROGRAM = 8;
    public const int ITEM_TYPE_INSTRUCTION = 9;
    public const int ITEM_TYPE_PROGRAM_PYTHON = 10;
    public const int ITEM_TYPE_MACHINING = 11;
    public const int ITEM_TYPE_BALLBARVALIDATION = 12;
    public const int ITEM_TYPE_CALIBPROJECT = 13;
    public const int ITEM_TYPE_VALID_ISO9283 = 14;

    // Instruction types
    public const int INS_TYPE_INVALID = -1;
    public const int INS_TYPE_MOVE = 0;
    public const int INS_TYPE_MOVEC = 1;
    public const int INS_TYPE_CHANGESPEED = 2;
    public const int INS_TYPE_CHANGEFRAME = 3;
    public const int INS_TYPE_CHANGETOOL = 4;
    public const int INS_TYPE_CHANGEROBOT = 5;
    public const int INS_TYPE_PAUSE = 6;
    public const int INS_TYPE_EVENT = 7;
    public const int INS_TYPE_CODE = 8;
    public const int INS_TYPE_PRINT = 9;

    // Move types
    public const int MOVE_TYPE_INVALID = -1;
    public const int MOVE_TYPE_JOINT = 1;
    public const int MOVE_TYPE_LINEAR = 2;
    public const int MOVE_TYPE_CIRCULAR = 3;

    // Station parameters request
    public const string PATH_OPENSTATION = "PATH_OPENSTATION";
    public const string FILE_OPENSTATION = "FILE_OPENSTATION";
    public const string PATH_DESKTOP = "PATH_DESKTOP";

    // Script execution types
    public const int RUNMODE_SIMULATE = 1;                      // performs the simulation moving the robot (default)
    public const int RUNMODE_QUICKVALIDATE = 2;                 // performs a quick check to validate the robot movements
    public const int RUNMODE_MAKE_ROBOTPROG = 3;                // makes the robot program
    public const int RUNMODE_MAKE_ROBOTPROG_AND_UPLOAD = 4;     // makes the robot program and updates it to the robot
    public const int RUNMODE_MAKE_ROBOTPROG_AND_START = 5;      // makes the robot program and starts it on the robot (independently from the PC)
    public const int RUNMODE_RUN_ROBOT = 6;                     // moves the real robot from the PC (PC is the client, the robot behaves like a server)

    // Program execution type
    public const int PROGRAM_RUN_ON_SIMULATOR = 1;        // Set the program to run on the simulator
    public const int PROGRAM_RUN_ON_ROBOT = 2;            // Set the program to run on the robot

    // TCP calibration types
    public const int CALIBRATE_TCP_BY_POINT = 0;
    public const int CALIBRATE_TCP_BY_PLANE = 1;

    // projection types (for AddCurve)
    public const int PROJECTION_NONE = 0; // No curve projection
    public const int PROJECTION_CLOSEST = 1; // The projection will the closest point on the surface
    public const int PROJECTION_ALONG_NORMAL = 2; // The projection will be done along the normal.
    public const int PROJECTION_ALONG_NORMAL_RECALC = 3; // The projection will be done along the normal. Furthermore, the normal will be recalculated according to the surface normal.

    // Euler type
    public const int EULER_RX_RYp_RZpp = 0; // generic
    public const int EULER_RZ_RYp_RXpp = 1; // ABB RobotStudio
    public const int EULER_RZ_RYp_RZpp = 2; // Kawasaki, Adept, Staubli
    public const int EULER_RZ_RXp_RZpp = 3; // CATIA, SolidWorks
    public const int EULER_RX_RY_RZ = 4; // Fanuc, Kuka, Motoman, Nachi
    public const int EULER_RZ_RY_RX = 5; // CRS
    public const int EULER_QUEATERNION = 6; // ABB Rapid

    // State of the RoboDK window
    public const int WINDOWSTATE_HIDDEN = -1;
    public const int WINDOWSTATE_SHOW = 0;
    public const int WINDOWSTATE_MINIMIZED = 1;
    public const int WINDOWSTATE_NORMAL = 2;
    public const int WINDOWSTATE_MAXIMIZED = 3;
    public const int WINDOWSTATE_FULLSCREEN = 4;
    public const int WINDOWSTATE_CINEMA = 5;
    public const int WINDOWSTATE_FULLSCREEN_CINEMA = 6;



    //string APPLICATION_DIR = "C:/RoboDK/bin/RoboDK.exe"; // file path to the robodk program (executable)
    string APPLICATION_DIR = "E:/install/RoboDK/bin/RoboDK.exe"; //邢双的RoboDK的exe路径
    int SAFE_MODE = 1;                      // checks that provided items exist in memory
    int AUTO_UPDATE = 0;                    // if AUTO_UPDATE is zero, the scene is rendered after every function call  
    int TIMEOUT = 10 * 1000;                    // timeout for communication, in seconds
    Socket COM;                             // tcpip com
    string IP = "localhost";                // IP address of the simulator (localhost if it is the same computer), otherwise, use RL = Robolink('yourip') to set to a different IP
    int PORT_START = 20500;                 // port to start looking for app connection
    int PORT_END = 20500;                   // port to stop looking for app connection
    bool START_HIDDEN = false;              // forces to start hidden. ShowRoboDK must be used to show the window
    int PORT = -1;                          // port where connection succeeded
    int PORT_FORCED = -1;                   // port to force RoboDK to start listening

    //Returns 1 if connection is valid, returns 0 if connection is invalid
    bool is_connected( )
    {
        return COM.Connected;
    }


    /// <summary>
    /// 关闭RoboDK通信
    /// </summary>
    public void Close( )
    {
        if (COM.Connected)
            COM.Close();
    }

    /// <summary>
    /// Checks if the object is currently linked to RoboDK
    /// </summary>
    /// <returns></returns>
    public bool Connected( )
    {
        //return COM.Connected;//does not work well
        bool part1 = COM.Poll(1000, SelectMode.SelectRead);
        bool part2 = COM.Available == 0;
        if (part1 && part2)
            return false;
        else
            return true;
    }

    //If we are not connected it will attempt a connection, if it fails, it will throw an error
    void check_connection( )
    {
        if (!is_connected() && !Connect())
        {
            throw new RDKException("Can't connect to RoboDK library");
        }
    }

    // checks the status of the connection
    int check_status( )
    {
        int status = rec_int();
        if (status > 0 && status < 10)
        {
            string strproblems;
            strproblems = "Unknown error";
            if (status == 1)
            {
                strproblems = "Invalid item provided: The item identifier provided is not valid or it does not exist.";
            }
            else if (status == 2)
            {//output warning
                strproblems = rec_line();
                //print("WARNING: " + strproblems);
                //#warn(strproblems)# does not show where is the problem...
                return 0;
            }
            else if (status == 3)
            { // output error
                strproblems = rec_line();
                throw new RDKException(strproblems);
            }
            else if (status == 9)
            {
                strproblems = "Invalid license. Contact us at: info@robodk.com";
            }
            //print(strproblems);
            throw new RDKException(strproblems); //raise Exception(strproblems)
        }
        else if (status == 0)
        {
            // everything is OK
            //status = status
        }
        else
        {
            throw new RDKException("Problems running function"); //raise Exception('Problems running function');
        }
        return status;
    }

    //Formats the color in a vector of size 4x1 and ranges [0,1]
    bool check_color(double[] color)
    {
        if (color.Length < 4)
        {
            throw new RDKException("Invalid color. A color must be a 4-size double array [r,g,b,a]"); //raise Exception('Problems running function');
            return false;
        }
        return true;
    }

    //Sends a string of characters with a \\n
    void send_line(string line)
    {
        line.Replace('\n', ' ');// one new line at the end only!
        byte[] data = System.Text.Encoding.UTF8.GetBytes(line + "\n");
        COM.Send(data);
    }

    string rec_line( )
    {
        //Receives a string. It reads until if finds LF (\\n)
        byte[] buffer = new byte[1];
        int bytesread = COM.Receive(buffer, 1, SocketFlags.None);
        string line = "";
        while (bytesread > 0 && buffer[0] != '\n')
        {
            line = line + System.Text.Encoding.UTF8.GetString(buffer);
            bytesread = COM.Receive(buffer, 1, SocketFlags.None);
        }
        return line;
    }

    //Sends an item pointer
    void send_item(Item item)
    {
        byte[] bytes;
        if (item == null)
        {
            bytes = BitConverter.GetBytes(((UInt64)0));
        }
        else
        {
            bytes = BitConverter.GetBytes((UInt64)item.get_item());
        }
        if (bytes.Length != 8)
        {
            throw new RDKException("API error");
        }
        Array.Reverse(bytes);
        COM.Send(bytes);
    }

    //Receives an item pointer
    Item rec_item( )
    {
        byte[] buffer1 = new byte[8];
        byte[] buffer2 = new byte[4];
        int read1 = COM.Receive(buffer1, 8, SocketFlags.None);
        int read2 = COM.Receive(buffer2, 4, SocketFlags.None);
        if (read1 != 8 || read2 != 4)
        {
            return null;
        }
        Array.Reverse(buffer1);
        Array.Reverse(buffer2);
        UInt64 item = BitConverter.ToUInt64(buffer1, 0);
        //Console.WriteLine("Received item: " + item.ToString());
        Int32 type = BitConverter.ToInt32(buffer2, 0);
        return new Item(this, item, type);
    }

    void send_pose(Mat pose)
    {
        if (!pose.IsHomogeneous())
        {
            // warning!!
            return;
        }
        const int nvalues = 16;
        byte[] bytesarray = new byte[8 * nvalues];
        int cnt = 0;
        for (int j = 0; j < pose.cols; j++)
        {
            for (int i = 0; i < pose.rows; i++)
            {
                byte[] onedouble = BitConverter.GetBytes((double)pose[i, j]);
                Array.Reverse(onedouble);
                Array.Copy(onedouble, 0, bytesarray, cnt * 8, 8);
                cnt = cnt + 1;
            }
        }
        COM.Send(bytesarray, 8 * nvalues, SocketFlags.None);
    }

    Mat rec_pose( )
    {
        Mat pose = new Mat(4, 4);
        byte[] bytes = new byte[16 * 8];
        int nbytes = COM.Receive(bytes, 16 * 8, SocketFlags.None);
        if (nbytes != 16 * 8)
        {
            throw new RDKException("Invalid pose sent"); //raise Exception('Problems running function');
        }
        int cnt = 0;
        for (int j = 0; j < pose.cols; j++)
        {
            for (int i = 0; i < pose.rows; i++)
            {
                byte[] onedouble = new byte[8];
                Array.Copy(bytes, cnt, onedouble, 0, 8);
                Array.Reverse(onedouble);
                pose[i, j] = BitConverter.ToDouble(onedouble, 0);
                cnt = cnt + 8;
            }
        }
        return pose;
    }

    void send_xyz(double[] xyzpos)
    {
        for (int i = 0; i < 3; i++)
        {
            byte[] bytes = BitConverter.GetBytes((double)xyzpos[i]);
            Array.Reverse(bytes);
            COM.Send(bytes, 8, SocketFlags.None);
        }
    }
    void rec_xyz(double[] xyzpos)
    {
        byte[] bytes = new byte[3 * 8];
        int nbytes = COM.Receive(bytes, 3 * 8, SocketFlags.None);
        if (nbytes != 3 * 8)
        {
            throw new RDKException("Invalid pose sent"); //raise Exception('Problems running function');
        }
        for (int i = 0; i < 3; i++)
        {
            byte[] onedouble = new byte[8];
            Array.Copy(bytes, i * 8, onedouble, 0, 8);
            Array.Reverse(onedouble);
            xyzpos[i] = BitConverter.ToDouble(onedouble, 0);
        }
    }

    void send_int(Int32 number)
    {
        byte[] bytes = BitConverter.GetBytes(number);
        Array.Reverse(bytes); // convert from big endian to little endian
        COM.Send(bytes);
    }

    Int32 rec_int( )
    {
        byte[] bytes = new byte[4];
        int read = COM.Receive(bytes, 4, SocketFlags.None);
        if (read < 4)
        {
            return 0;
        }
        Array.Reverse(bytes); // convert from little endian to big endian
        return BitConverter.ToInt32(bytes, 0);
    }

    // Sends an array of doubles
    void send_array(double[] values)
    {
        if (values == null)
        {
            send_int(0);
            return;
        }
        int nvalues = values.Length;
        send_int(nvalues);
        byte[] bytesarray = new byte[8 * nvalues];
        for (int i = 0; i < nvalues; i++)
        {
            byte[] onedouble = BitConverter.GetBytes(values[i]);
            Array.Reverse(onedouble);
            Array.Copy(onedouble, 0, bytesarray, i * 8, 8);
        }
        COM.Send(bytesarray, 8 * nvalues, SocketFlags.None);
    }

    // Receives an array of doubles
    double[] rec_array( )
    {
        int nvalues = rec_int();
        if (nvalues > 0)
        {
            double[] values = new double[nvalues];
            byte[] bytes = new byte[nvalues * 8];
            int read = COM.Receive(bytes, nvalues * 8, SocketFlags.None);
            for (int i = 0; i < nvalues; i++)
            {
                byte[] onedouble = new byte[8];
                Array.Copy(bytes, i * 8, onedouble, 0, 8);
                Array.Reverse(onedouble);
                values[i] = BitConverter.ToDouble(onedouble, 0);
            }
            return values;
        }
        return null;
    }

    // sends a 2 dimensional matrix
    void send_matrix(Mat mat)
    {
        send_int(mat.rows);
        send_int(mat.cols);
        for (int j = 0; j < mat.cols; j++)
        {
            for (int i = 0; i < mat.rows; i++)
            {
                byte[] bytes = BitConverter.GetBytes((double)mat[i, j]);
                Array.Reverse(bytes);
                COM.Send(bytes, 8, SocketFlags.None);
            }
        }

    }

    // receives a 2 dimensional matrix (nxm)
    Mat rec_matrix( )
    {
        int size1 = rec_int();
        int size2 = rec_int();
        int recvsize = size1 * size2 * 8;
        byte[] bytes = new byte[recvsize];
        Mat mat = new Mat(size1, size2);
        int BUFFER_SIZE = 256;
        int received = 0;
        if (recvsize > 0)
        {
            int to_receive = Math.Min(recvsize, BUFFER_SIZE);
            while (to_receive > 0)
            {
                int nbytesok = COM.Receive(bytes, received, to_receive, SocketFlags.None);
                if (nbytesok <= 0)
                {
                    throw new RDKException("Can't receive matrix properly"); //raise Exception('Problems running function');
                }
                received = received + nbytesok;
                to_receive = Math.Min(recvsize - received, BUFFER_SIZE);
            }
        }
        int cnt = 0;
        for (int j = 0; j < mat.cols; j++)
        {
            for (int i = 0; i < mat.rows; i++)
            {
                byte[] onedouble = new byte[8];
                Array.Copy(bytes, cnt, onedouble, 0, 8);
                Array.Reverse(onedouble);
                mat[i, j] = BitConverter.ToDouble(onedouble, 0);
                cnt = cnt + 8;
            }
        }
        return mat;
    }

    // private move type, to be used by public methods (MoveJ  and MoveL)
    void moveX(Item target, double[] joints, Mat mat_target, Item itemrobot, int movetype, bool blocking = true)
    {
        itemrobot.WaitMove();
        string command = "MoveX";
        send_line(command);
        send_int(movetype);
        if (target != null)
        {
            send_int(3);
            send_array(null);
            send_item(target);
        }
        else if (joints != null)
        {
            send_int(1);
            send_array(joints);
            send_item(null);
        }
        else if (mat_target != null && mat_target.IsHomogeneous())
        {
            send_int(2);
            send_array(mat_target.ToDoubles());
            send_item(null);
        }
        else
        {
            throw new RDKException("Invalid target type"); //raise Exception('Problems running function');
        }
        send_item(itemrobot);
        check_status();
        if (blocking)
        {
            itemrobot.WaitMove();
        }
    }
    // private move type, to be used by public methods (MoveJ  and MoveL)
    void moveC_private(Item target1, double[] joints1, Mat mat_target1, Item target2, double[] joints2, Mat mat_target2, Item itemrobot, bool blocking = true)
    {
        itemrobot.WaitMove();
        string command = "MoveC";
        send_line(command);
        send_int(3);
        if (target1 != null)
        {
            send_int(3);
            send_array(null);
            send_item(target1);
        }
        else if (joints1 != null)
        {
            send_int(1);
            send_array(joints1);
            send_item(null);
        }
        else if (mat_target1 != null && mat_target1.IsHomogeneous())
        {
            send_int(2);
            send_array(mat_target1.ToDoubles());
            send_item(null);
        }
        else
        {
            throw new RDKException("Invalid type of target 1");
        }
        /////////////////////////////////////
        if (target2 != null)
        {
            send_int(3);
            send_array(null);
            send_item(target2);
        }
        else if (joints2 != null)
        {
            send_int(1);
            send_array(joints2);
            send_item(null);
        }
        else if (mat_target2 != null && mat_target2.IsHomogeneous())
        {
            send_int(2);
            send_array(mat_target2.ToDoubles());
            send_item(null);
        }
        else
        {
            throw new RDKException("Invalid type of target 2");
        }
        /////////////////////////////////////
        send_item(itemrobot);
        check_status();
        if (blocking)
        {
            itemrobot.WaitMove();
        }
    }

    //%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%   

    /// <summary>
    /// Creates a link with RoboDK
    /// </summary>
    /// <param name="robodk_ip"></param>
    /// <param name="start_hidden"></param>
    /// <param name="com_port"></param>
    public RoboDK(string robodk_ip = "localhost", bool start_hidden = false, int com_port = -1)
    {
        //A connection is attempted upon creation of the object"""
        IP = robodk_ip;
        START_HIDDEN = start_hidden;
        if (com_port > 0)
        {
            PORT_FORCED = com_port;
            PORT_START = com_port;
            PORT_END = com_port;
        }
        Connect();
    }

    private bool Set_connection_params(int safe_mode = 1, int auto_update = 0, int timeout = -1)
    {
        //Sets some behavior parameters: SAFE_MODE, AUTO_UPDATE and TIMEOUT.
        SAFE_MODE = safe_mode;
        AUTO_UPDATE = auto_update;
        if (timeout >= 0)
        {
            TIMEOUT = timeout;
        }
        send_line("CMD_START");
        send_line(Convert.ToString(SAFE_MODE) + " " + Convert.ToString(AUTO_UPDATE));
        string response = rec_line();
        if (response == "READY")
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Starts the link with RoboDK (automatic upon creation of the object)
    /// </summary>
    /// <returns></returns>
    public bool Connect( )
    {
        //Establishes a connection with robodk. robodk must be running, otherwise, the variable APPLICATION_DIR must be set properly.
        bool connected = false;
        int port;
        for (int i = 0; i < 2; i++)
        {
            for (port = PORT_START; port <= PORT_END; port++)
            {
                COM = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
                //COM = new Socket(SocketType.Stream, ProtocolType.IPv4);
                COM.SendTimeout = 1000;
                COM.ReceiveTimeout = 1000;
                try
                {
                    COM.Connect(IP, port);
                    connected = is_connected();
                    if (connected)
                    {
                        COM.SendTimeout = TIMEOUT;
                        COM.ReceiveTimeout = TIMEOUT;
                        break;
                    }
                }
                catch (Exception e)
                {
                    //connected = false;
                }
            }
            if (connected)
            {
                PORT = port;
                break;
            }
            else
            {
                if (IP != "localhost")
                {
                    break;
                }
                string arguments = "";
                if (PORT_FORCED > 0)
                {
                    arguments = arguments + "/PORT=" + PORT_FORCED.ToString();
                }
                if (START_HIDDEN)
                {
                    arguments = arguments + "/NOSPLASH /NOSHOW";
                }
                System.Diagnostics.Process.Start(APPLICATION_DIR, arguments);
                System.Threading.Thread.Sleep(2000);                // 由于robot启动比较慢，原来的1s时间不够，会导致启动两次，而2s就可以避免这个问题，邢双修改；
            }
        }
        if (connected && !Set_connection_params())
        {
            connected = false;
        }
        return connected;
    }


    // %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
    // public methods
    /// <summary>
    /// Returns an item by its name. If there is no exact match it will return the last closest match.
    /// </summary>
    /// <param name="name">Item name</param>
    /// <param name="type">Filter by item type RoboDK.ITEM_TYPE_...</param>
    /// <returns></returns>
    public Item getItem(string name, int itemtype = -1)
    {
        check_connection();
        string command;
        if (itemtype < 0)
        {
            command = "G_Item";
            send_line(command);
            send_line(name);
        }
        else
        {
            command = "G_Item2";
            send_line(command);
            send_line(name);
            send_int(itemtype);
        }
        Item item = rec_item();
        check_status();
        return item;
    }

    /////// add more methods

    /// <summary>
    /// Shows a RoboDK popup to select one object from the open station.
    /// An item type can be specified to filter desired items. If no type is specified, all items are selectable.
    /// </summary>
    /// <param name="message">Message to pop up</param>
    /// <param name="itemtype">optionally filter by RoboDK.ITEM_TYPE_*</param>
    /// <returns></returns>
    public Item ItemUserPick(string message = "Pick one item", int itemtype = -1)
    {
        check_connection();
        string command = "PickItem";
        send_line(command);
        send_line(message);
        send_int(itemtype);
        COM.ReceiveTimeout = 3600 * 1000;
        Item item = rec_item();
        COM.ReceiveTimeout = TIMEOUT;
        check_status();
        return item;
    }

    /// <summary>
    /// Shows or raises the RoboDK window
    /// </summary>
    public void ShowRoboDK( )
    {
        check_connection();
        string command = "RAISE";
        send_line(command);
        check_status();
    }

    /// <summary>
    /// Hides the RoboDK window
    /// </summary>
    public void HideRoboDK( )
    {
        check_connection();
        string command = "HIDE";
        send_line(command);
        check_status();
    }

    /// <summary>
    /// Set the state of the RoboDK window
    /// </summary>
    /// <param name="windowstate"></param>
    public void setWindowState(int windowstate = WINDOWSTATE_NORMAL)
    {
        check_connection();
        string command = "S_WindowState";
        send_line(command);
        send_int(windowstate);
        check_status();
    }

    public void ShowMessage(string message)
    {
        check_connection();
        string command = "ShowMessage";
        send_line(command);
        send_line(message);
        COM.ReceiveTimeout = 3600 * 1000;
        check_status();
        COM.ReceiveTimeout = TIMEOUT;
    }

    /////////////// Add More methods

    /// <summary>
    /// Loads a file and attaches it to parent. It can be any file supported by robodk.
    /// </summary>
    /// <param name="filename">absolute path of the file</param>
    /// <param name="parent">parent to attach. Leave empty for new stations or to load an object at the station root</param>
    /// <returns>Newly added object. Check with item.Valid() for a successful load</returns>
    public Item AddFile(string filename, Item parent = null)
    {
        check_connection();
        string command = "Add";
        send_line(command);
        send_line(filename);
        send_item(parent);
        Item newitem = rec_item();
        check_status();
        return newitem;
    }

    /////////////// Add More methods

    /// <summary>
    /// Save an item to a file. If no item is provided, the open station is saved.
    /// </summary>
    /// <param name="filename">absolute path to save the file</param>
    /// <param name="itemsave">object or station to save. Leave empty to automatically save the current station.</param>
    public void Save(string filename, Item itemsave = null)
    {
        check_connection();
        string command = "Save";
        send_line(command);
        send_line(filename);
        send_item(itemsave);
        check_status();
    }

    /// <summary>
    /// Adds a curve provided point coordinates. The provided points must be a list of vertices. A vertex normal can be provided optionally.
    /// </summary>
    /// <param name="curve_points">matrix 3xN or 6xN -> N must be multiple of 3</param>
    /// <param name="reference_object">object to add the curve and/or project the curve to the surface</param>
    /// <param name="add_to_ref">If True, the curve will be added as part of the object in the RoboDK item tree (a reference object must be provided)</param>
    /// <param name="projection_type">Type of projection. For example: PROJECTION_ALONG_NORMAL_RECALC will project along the point normal and recalculate the normal vector on the surface projected.</param>
    /// <returns>added object/curve (null if failed)</returns>
    public Item AddCurve(Mat curve_points, Item reference_object = null, bool add_to_ref = false, int projection_type = PROJECTION_ALONG_NORMAL_RECALC)
    {
        check_connection();
        string command = "AddWire";
        send_line(command);
        send_matrix(curve_points);
        send_item(reference_object);
        send_int(add_to_ref ? 1 : 0);
        send_int(projection_type);
        Item newitem = rec_item();
        check_status();
        return newitem;
    }

    /// <summary>
    /// Projects a point given its coordinates. The provided points must be a list of [XYZ] coordinates. Optionally, a vertex normal can be provided [XYZijk].
    /// </summary>
    /// <param name="points">matrix 3xN or 6xN -> list of points to project</param>
    /// <param name="object_project">object to project</param>
    /// <param name="projection_type">Type of projection. For example: PROJECTION_ALONG_NORMAL_RECALC will project along the point normal and recalculate the normal vector on the surface projected.</param>
    /// <returns></returns>
    public Mat ProjectPoints(Mat points, Item object_project, int projection_type = PROJECTION_ALONG_NORMAL_RECALC)
    {
        check_connection();
        string command = "ProjectPoints";
        send_line(command);
        send_matrix(points);
        send_item(object_project);
        send_int(projection_type);
        Mat projected_points = rec_matrix();
        check_status();
        return projected_points;
    }

    /// <summary>
    /// Closes the current station without suggesting to save
    /// </summary>
    public void CloseStation( )
    {
        check_connection();
        string command = "Remove";
        send_line(command);
        send_item(new Item(this));
        check_status();
    }

    /// <summary>
    /// Adds a new target that can be reached with a robot.
    /// </summary>
    /// <param name="name">name of the target</param>
    /// <param name="itemparent">parent to attach to (such as a frame)</param>
    /// <param name="itemrobot">main robot that will be used to go to self target</param>
    /// <returns>the new target created</returns>
    public Item AddTarget(string name, Item itemparent = null, Item itemrobot = null)
    {
        check_connection();
        string command = "Add_TARGET";
        send_line(command);
        send_line(name);
        send_item(itemparent);
        send_item(itemrobot);
        Item newitem = rec_item();
        check_status();
        return newitem;
    }

    /// <summary>
    /// Adds a new Frame that can be referenced by a robot.
    /// </summary>
    /// <param name="name">name of the reference frame</param>
    /// <param name="itemparent">parent to attach to (such as the robot base frame)</param>
    /// <returns>the new reference frame created</returns>
    public Item AddFrame(string name, Item itemparent = null)
    {
        check_connection();
        string command = "Add_FRAME";
        send_line(command);
        send_line(name);
        send_item(itemparent);
        Item newitem = rec_item();
        check_status();
        return newitem;
    }

    /// <summary>
    /// Adds a new Frame that can be referenced by a robot.
    /// </summary>
    /// <param name="name">name of the program</param>
    /// <param name="itemparent">robot that will be used</param>
    /// <returns>the new program created</returns>
    public Item AddProgram(string name, Item itemrobot = null)
    {
        check_connection();
        string command = "Add_PROG";
        send_line(command);
        send_line(name);
        send_item(itemrobot);
        Item newitem = rec_item();
        check_status();
        return newitem;
    }


    //%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
    /// <summary>
    /// Adds a function call in the program output. RoboDK will handle the syntax when the code is generated for a specific robot. If the program exists it will also run the program in simulate mode.
    /// </summary>
    /// <param name="function_w_params">Function name with parameters (if any)</param>
    /// <returns></returns>
    public int RunProgram(string function_w_params)
    {
        return RunCode(function_w_params, true);
    }

    /// <summary>
    /// Adds code to run in the program output. If the program exists it will also run the program in simulate mode.
    /// </summary>
    /// <param name="code"></param>
    /// <param name="code_is_fcn_call"></param>
    /// <returns></returns>
    public int RunCode(string code, bool code_is_fcn_call = false)
    {
        check_connection();
        string command = "RunCode";
        send_line(command);
        send_int(code_is_fcn_call ? 1 : 0);
        send_line(code);
        int prog_status = rec_int();
        check_status();
        return prog_status;
    }

    /// <summary>
    /// Shows a message or a comment in the output robot program.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="message_is_comment"></param>
    public void RunMessage(string message, bool message_is_comment = false)
    {
        check_connection();
        string command = "RunMessage";
        send_line(command);
        send_int(message_is_comment ? 1 : 0);
        send_line(message);
        check_status();
    }

    /// <summary>
    /// Renders the scene. This function turns off rendering unless always_render is set to true.
    /// </summary>
    /// <param name="always_render"></param>
    public void Render(bool always_render = false)
    {
        bool auto_render = !always_render;
        check_connection();
        string command = "Render";
        send_line(command);
        send_int(auto_render ? 1 : 0);
        check_status();
    }

    /// <summary>
    /// Returns the number of pairs of objects that are currently in a collision state.
    /// </summary>
    /// <returns></returns>
    public int Collisions( )
    {
        check_connection();
        string command = "Collisions";
        send_line(command);
        int ncollisions = rec_int();
        check_status();
        return ncollisions;
    }

    /// <summary>
    /// Returns 1 if item1 and item2 collided. Otherwise returns 0.
    /// </summary>
    /// <param name="item1"></param>
    /// <param name="item2"></param>
    /// <returns></returns>
    public int Collision(Item item1, Item item2)
    {
        check_connection();
        string command = "Collided";
        send_line(command);
        send_item(item1);
        send_item(item2);
        int ncollisions = rec_int();
        check_status();
        return ncollisions;
    }

    /// <summary>
    /// Sets the current simulation speed. Set the speed to 1 for a real-time simulation. The slowest speed allowed is 0.001 times the real speed. Set to a high value (>100) for fast simulation results.
    /// </summary>
    /// <param name="speed"></param>
    public void setSimulationSpeed(double speed)
    {
        check_connection();
        string command = "SimulateSpeed";
        send_line(command);
        send_int((int)(speed * 1000.0));
        check_status();
    }

    /// <summary>
    /// Gets the current simulation speed. Set the speed to 1 for a real-time simulation.
    /// </summary>
    /// <returns></returns>
    public double SimulationSpeed( )
    {
        check_connection();
        string command = "GetSimulateSpeed";
        send_line(command);
        double speed = ((double)rec_int()) / 1000.0;
        check_status();
        return speed;
    }
    /// <summary>
    /// Sets the behavior of the RoboDK API. By default, robodk shows the path simulation for movement instructions (run_mode=1=RUNMODE_SIMULATE).
    /// Setting the run_mode to RUNMODE_QUICKVALIDATE allows performing a quick check to see if the path is feasible.
    /// if robot.Connect() is used, RUNMODE_RUN_FROM_PC is selected automatically.
    /// </summary>
    /// <param name="run_mode">int = RUNMODE
    /// RUNMODE_SIMULATE=1        performs the simulation moving the robot (default)
    /// RUNMODE_QUICKVALIDATE=2   performs a quick check to validate the robot movements
    /// RUNMODE_MAKE_ROBOTPROG=3  makes the robot program
    /// RUNMODE_RUN_REAL=4        moves the real robot is it is connected</param>
    public void setRunMode(int run_mode = 1)
    {
        check_connection();
        string command = "S_RunMode";
        send_line(command);
        send_int(run_mode);
        check_status();
    }

    /// <summary>
    /// Returns the behavior of the RoboDK API. By default, robodk shows the path simulation for movement instructions (run_mode=1)
    /// </summary>
    /// <returns>int = RUNMODE
    /// RUNMODE_SIMULATE=1        performs the simulation moving the robot (default)
    /// RUNMODE_QUICKVALIDATE=2   performs a quick check to validate the robot movements
    /// RUNMODE_MAKE_ROBOTPROG=3  makes the robot program
    /// RUNMODE_RUN_REAL=4        moves the real robot is it is connected</returns>
    public int RunMode( )
    {
        check_connection();
        string command = "G_RunMode";
        send_line(command);
        int runmode = rec_int();
        check_status();
        return runmode;
    }

    /// <summary>
    /// Returns the current joints of a list of robots.
    /// </summary>
    /// <param name="robot_item_list">list of robot items</param>
    /// <returns>list of robot joints (double x nDOF)</returns>
    public double[][] Joints(Item[] robot_item_list)
    {
        check_connection();
        string command = "G_ThetasList";
        send_line(command);
        int nrobs = robot_item_list.Length;
        send_int(nrobs);
        double[][] joints_list = new double[nrobs][];
        for (int i = 0; i < nrobs; i++)
        {
            send_item(robot_item_list[i]);
            joints_list[i] = rec_array();
        }
        check_status();
        return joints_list;
    }

    /// <summary>
    /// Sets the current robot joints for a list of robot items and a list of a set of joints.
    /// </summary>
    /// <param name="robot_item_list">list of robot items</param>
    /// <param name="joints_list">list of robot joints (double x nDOF)</param>
    public void setJoints(Item[] robot_item_list, double[][] joints_list)
    {
        int nrobs = Math.Min(robot_item_list.Length, joints_list.Length);
        check_connection();
        string command = "S_ThetasList";
        send_line(command);
        send_int(nrobs);
        for (int i = 0; i < nrobs; i++)
        {
            send_item(robot_item_list[i]);
            send_array(joints_list[i]);
        }
        check_status();
    }

    /// <summary>
    /// The Item class represents an item in RoboDK station. An item can be a robot, a frame, a tool, an object, a target, ... any item visible in the station tree.
    /// An item can also be seen as a node where other items can be attached to (child items).
    /// Every item has one parent item/node and can have one or more child items/nodes
    /// RoboLinkItem is a "friend" class of RoboLink.
    /// </summary>
    public class Item
    {
        private UInt64 item = 0;
        private RoboDK link; // pointer to the RoboLink connection
        int type = -1;
        string name;

        public Item(RoboDK connection_link, UInt64 item_ptr = 0, int itemtype = -1)
        {
            item = item_ptr;
            link = connection_link;
            type = itemtype;
        }

        public UInt64 get_item( )
        {
            return item;
        }

        public string ToString2( )
        {
            if (Valid())
            {
                return String.Format("RoboDK item {0} of type {1}", item, type);
            }
            else
            {
                return "RoboDK item (INVALID)";
            }
        }

        /// <summary>
        /// Returns an integer that represents the type of the item (robot, object, tool, frame, ...)
        /// Compare the returned value to ITEM_CASE_* variables
        /// </summary>
        /// <param name="item_other"></param>
        /// <returns></returns>
        public bool Equals(Item item_other)
        {
            return this.item == item_other.item;
        }

        /// <summary>
        /// Returns the RoboDK link Robolink().
        /// </summary>
        /// <returns></returns>
        public RoboDK RL( )
        {
            return link;
        }

        //////// GENERIC ITEM CALLS
        /// <summary>
        /// Returns the type of an item (robot, object, target, reference frame, ...)
        /// </summary>
        /// <returns></returns>
        public int Type( )
        {
            link.check_connection();
            string command = "G_Item_Type";
            link.send_line(command);
            link.send_item(this);
            int itemtype = link.rec_int();
            link.check_status();
            return itemtype;
        }

        ////// add more methods

        /// <summary>
        /// Save a station or object to a file
        /// </summary>
        /// <param name="filename"></param>
        public void Save(string filename)
        {
            link.Save(filename, this);
        }

        /// <summary>
        /// Deletes an item and its childs from the station.
        /// </summary>
        public void Delete( )
        {
            link.check_connection();
            string command = "Remove";
            link.send_line(command);
            link.send_item(this);
            link.check_status();
            item = 0;
        }

        /// <summary>
        /// Checks if the item is valid. An invalid item will be returned by an unsuccessful function call.
        /// </summary>
        /// <returns>true if valid, false if invalid</returns>
        public bool Valid( )
        {
            if (item == 0)
            {
                return false;
            }
            return true;
        }

        ////// add more methods
        /// <summary>
        /// Returns a list of the item childs that are attached to the provided item.
        /// </summary>
        /// <returns>item x n -> list of child items</returns>
        public Item[] Childs( )
        {
            link.check_connection();
            string command = "G_Childs";
            link.send_line(command);
            link.send_item(this);
            int nitems = link.rec_int();
            Item[] itemlist = new Item[nitems];
            for (int i = 0; i < nitems; i++)
            {
                itemlist[i] = link.rec_item();
            }
            link.check_status();
            return itemlist;
        }

        /// <summary>
        /// Returns 1 if the item is visible, otherwise, returns 0.
        /// </summary>
        /// <returns>true if visible, false if not visible</returns>
        public bool Visible( )
        {
            link.check_connection();
            string command = "G_Visible";
            link.send_line(command);
            link.send_item(this);
            int visible = link.rec_int();
            link.check_status();
            return (visible != 0);
        }
        /// <summary>
        /// Sets the item visiblity status
        /// </summary>
        /// <param name="visible"></param>
        /// <param name="visible_frame">srt the visible reference frame (1) or not visible (0)</param>
        public void setVisible(bool visible, int visible_frame = -1)
        {
            if (visible_frame < 0)
            {
                visible_frame = visible ? 1 : 0;
            }
            link.check_connection();
            string command = "S_Visible";
            link.send_line(command);
            link.send_item(this);
            link.send_int(visible ? 1 : 0);
            link.send_int(visible_frame);
            link.check_status();
        }

        /// <summary>
        /// Returns the name of an item. The name of the item is always displayed in the RoboDK station tree
        /// </summary>
        /// <returns>name of the item</returns>
        public string Name( )
        {
            link.check_connection();
            string command = "G_Name";
            link.send_line(command);
            link.send_item(this);
            name = link.rec_line();
            link.check_status();
            return name;
        }

        /// <summary>
        /// Set the name of a RoboDK item.
        /// </summary>
        /// <param name="name"></param>
        public void setName(string name)
        {
            link.check_connection();
            string command = "S_Name";
            link.send_line(command);
            link.send_item(this);
            link.send_line(name);
            link.check_status();
        }

        // add more methods

        /// <summary>
        /// Sets the local position (pose) of an object, target or reference frame. For example, the position of an object/frame/target with respect to its parent.
        /// If a robot is provided, it will set the pose of the end efector.
        /// </summary>
        /// <param name="pose">4x4 homogeneous matrix</param>
        public void setPose(Mat pose)
        {
            link.check_connection();
            string command = "S_Hlocal";
            link.send_line(command);
            link.send_item(this);
            link.send_pose(pose);
            link.check_status();
        }

        /// <summary>
        /// Returns the local position (pose) of an object, target or reference frame. For example, the position of an object/frame/target with respect to its parent.
        /// If a robot is provided, it will get the pose of the end efector
        /// </summary>
        /// <returns>4x4 homogeneous matrix (pose)</returns>
        public Mat Pose( )
        {
            link.check_connection();
            string command = "G_Hlocal";
            link.send_line(command);
            link.send_item(this);
            Mat pose = link.rec_pose();
            link.check_status();
            return pose;
        }

        /// <summary>
        /// Sets the position (pose) the object geometry with respect to its own reference frame. This procedure works for tools and objects.
        /// </summary>
        /// <param name="pose">4x4 homogeneous matrix</param>
        public void setGeometryPose(Mat pose)
        {
            link.check_connection();
            string command = "S_Hgeom";
            link.send_line(command);
            link.send_item(this);
            link.send_pose(pose);
            link.check_status();
        }

        /// <summary>
        /// Returns the position (pose) the object geometry with respect to its own reference frame. This procedure works for tools and objects.
        /// </summary>
        /// <returns>4x4 homogeneous matrix (pose)</returns>
        public Mat GeometryPose( )
        {
            link.check_connection();
            string command = "G_Hgeom";
            link.send_line(command);
            link.send_item(this);
            Mat pose = link.rec_pose();
            link.check_status();
            return pose;
        }

        /// <summary>
        /// Sets the tool pose of a tool item. If a robot is provided it will set the tool pose of the active tool held by the robot.
        /// </summary>
        /// <param name="pose">4x4 homogeneous matrix (pose)</param>
        public void setHtool(Mat pose)
        {
            link.check_connection();
            string command = "S_Htool";
            link.send_line(command);
            link.send_item(this);
            link.send_pose(pose);
            link.check_status();
        }

        /// <summary>
        /// Returns the tool pose of an item. If a robot is provided it will get the tool pose of the active tool held by the robot.
        /// </summary>
        /// <returns>4x4 homogeneous matrix (pose)</returns>
        public Mat Htool( )
        {
            link.check_connection();
            string command = "G_Htool";
            link.send_line(command);
            link.send_item(this);
            Mat pose = link.rec_pose();
            link.check_status();
            return pose;
        }

        /// <summary>
        /// Sets the global position (pose) of an item. For example, the position of an object/frame/target with respect to the station origin.
        /// </summary>
        /// <param name="pose">4x4 homogeneous matrix (pose)</param>
        public void setPoseAbs(Mat pose)
        {
            link.check_connection();
            string command = "S_Hlocal_Abs";
            link.send_line(command);
            link.send_item(this);
            link.send_pose(pose);
            link.check_status();

        }

        /// <summary>
        /// Returns the global position (pose) of an item. For example, the position of an object/frame/target with respect to the station origin.
        /// </summary>
        /// <returns>4x4 homogeneous matrix (pose)</returns>
        public Mat PoseAbs( )
        {
            link.check_connection();
            string command = "G_Hlocal_Abs";
            link.send_line(command);
            link.send_item(this);
            Mat pose = link.rec_pose();
            link.check_status();
            return pose;
        }

        /// <summary>
        /// Changes the color of a robot/object/tool. A color must must in the format COLOR=[R,G,B,(A=1)] where all values range from 0 to 1.
        /// Alpha (A) defaults to 1 (100% opaque). Set A to 0 to make an object transparent.
        /// </summary>
        /// <param name="tocolor">color to change to</param>
        /// <param name="fromcolor">filter by this color</param>
        /// <param name="tolerance">optional tolerance to use if a color filter is used (defaults to 0.1)</param>
        public void Recolor(double[] tocolor, double[] fromcolor = null, double tolerance = 0.1)
        {
            link.check_connection();
            if (fromcolor == null)
            {
                fromcolor = new double[] { 0, 0, 0, 0 };
                tolerance = 2;
            }
            link.check_color(tocolor);
            link.check_color(fromcolor);
            string command = "Recolor";
            link.send_line(command);
            link.send_item(this);
            double[] combined = new double[9];
            combined[0] = tolerance;
            Array.Copy(fromcolor, 0, combined, 1, 4);
            Array.Copy(tocolor, 0, combined, 5, 4);
            link.send_array(combined);
            link.check_status();
        }

        /// <summary>
        /// Apply a scale to an object to make it bigger or smaller.
        /// The scale can be uniform (if scale is a float value) or per axis (if scale is a vector).
        /// </summary>
        /// <param name="scale">scale to apply as [scale_x, scale_y, scale_z]</param>
        public void Scale(double[] scale)
        {
            link.check_connection();
            if (scale.Length != 3)
            {
                throw new RDKException("scale must be a single value or a 3-vector value");
            }
            string command = "Scale";
            link.send_line(command);
            link.send_item(this);
            link.send_array(scale);
            link.check_status();
        }

        /// <summary>
        /// Adds a curve provided point coordinates. The provided points must be a list of vertices. A vertex normal can be provided optionally.
        /// </summary>
        /// <param name="curve_points">matrix 3xN or 6xN -> N must be multiple of 3</param>
        /// <param name="add_to_ref">add_to_ref -> If True, the curve will be added as part of the object in the RoboDK item tree</param>
        /// <param name="projection_type">Type of projection. For example: PROJECTION_ALONG_NORMAL_RECALC will project along the point normal and recalculate the normal vector on the surface projected.</param>
        /// <returns>returns the object where the curve was added or null if failed</returns>
        public Item AddCurve(Mat curve_points, bool add_to_ref = false, int projection_type = PROJECTION_ALONG_NORMAL_RECALC)
        {
            return link.AddCurve(curve_points, this, add_to_ref, projection_type);
        }

        /// <summary>
        /// Projects a point to the object given its coordinates. The provided points must be a list of [XYZ] coordinates. Optionally, a vertex normal can be provided [XYZijk].
        /// </summary>
        /// <param name="points">matrix 3xN or 6xN -> list of points to project</param>
        /// <param name="projection_type">projection_type -> Type of projection. For example: PROJECTION_ALONG_NORMAL_RECALC will project along the point normal and recalculate the normal vector on the surface projected.</param>
        /// <returns>projected points (empty matrix if failed)</returns>
        public Mat ProjectPoints(Mat points, int projection_type = PROJECTION_ALONG_NORMAL_RECALC)
        {
            return link.ProjectPoints(points, this, projection_type);
        }

        //"""Target item calls"""

        /// <summary>
        /// Sets a target as a cartesian target. A cartesian target moves to cartesian coordinates.
        /// </summary>
        public void setAsCartesianTarget( )
        {
            link.check_connection();
            string command = "S_Target_As_RT";
            link.send_line(command);
            link.send_item(this);
            link.check_status();
        }

        /// <summary>
        /// Sets a target as a joint target. A joint target moves to a joints position without regarding the cartesian coordinates.
        /// </summary>
        public void setAsJointTarget( )
        {
            link.check_connection();
            string command = "S_Target_As_JT";
            link.send_line(command);
            link.send_item(this);
            link.check_status();
        }

        //#####Robot item calls####

        /// <summary>
        /// Returns the current joints of a robot or the joints of a target. If the item is a cartesian target, it returns the preferred joints (configuration) to go to that cartesian position.
        /// </summary>
        /// <returns>double x n -> joints matrix</returns>
        public double[] Joints( )
        {
            link.check_connection();
            string command = "G_Thetas";
            link.send_line(command);
            link.send_item(this);
            double[] joints = link.rec_array();
            link.check_status();
            return joints;
        }

        // add more methods

        /// <summary>
        /// Returns the home joints of a robot. These joints can be manually set in the robot "Parameters" menu, then select "Set home position"
        /// </summary>
        /// <returns>double x n -> joints array</returns>
        public double[] JointsHome( )
        {
            link.check_connection();
            string command = "G_Home";
            link.send_line(command);
            link.send_item(this);
            double[] joints = link.rec_array();
            link.check_status();
            return joints;
        }

        /// <summary>
        /// Sets the current joints of a robot or the joints of a target. It the item is a cartesian target, it returns the preferred joints (configuration) to go to that cartesian position.
        /// </summary>
        /// <param name="joints"></param>
        public void setJoints(double[] joints)
        {
            link.check_connection();
            string command = "S_Thetas";
            link.send_line(command);
            link.send_array(joints);
            link.send_item(this);
            link.check_status();
        }

        /// <summary>
        /// Returns the joint limits of a robot
        /// </summary>
        /// <param name="lower_limits"></param>
        /// <param name="upper_limits"></param>
        public void JointLimits(ref double[] lower_limits, ref double[] upper_limits)
        {
            link.check_connection();
            string command = "G_RobLimits";
            link.send_line(command);
            link.send_item(this);
            lower_limits = link.rec_array();
            upper_limits = link.rec_array();
            double joints_type = link.rec_int() / 1000.0;
            link.check_status();
        }

        /// <summary>
        /// Sets the robot of a program or a target. You must set the robot linked to a program or a target every time you copy paste these objects.
        /// If the robot is not provided, the first available robot will be chosen automatically.
        /// </summary>
        /// <param name="robot">Robot item</param>
        public void setRobot(Item robot = null)
        {
            link.check_connection();
            string command = "S_Robot";
            link.send_line(command);
            link.send_item(this);
            link.send_item(robot);
            link.check_status();
        }

        /// <summary>
        /// Sets the frame of a robot (user frame). The frame can be either an item or a 4x4 Matrix.
        /// If "frame" is an item, it links the robot to the frame item. If frame is a 4x4 Matrix, it updates the linked pose of the robot frame.
        /// </summary>
        /// <param name="frame">item/pose -> frame item or 4x4 Matrix (pose of the reference frame)</param>
        public void setFrame(Item frame)
        {
            link.check_connection();
            string command = "S_Frame_ptr";
            link.send_line(command);
            link.send_item(frame);
            link.send_item(this);
            link.check_status();
        }

        /// <summary>
        /// Sets the frame of a robot (user frame). The frame can be either an item or a 4x4 Matrix.
        /// If "frame" is an item, it links the robot to the frame item. If frame is a 4x4 Matrix, it updates the linked pose of the robot frame.
        /// </summary>
        /// <param name="frame">item/pose -> frame item or 4x4 Matrix (pose of the reference frame)</param>
        public void setFrame(Mat frame)
        {
            link.check_connection();
            string command = "S_Frame";
            link.send_line(command);
            link.send_pose(frame);
            link.send_item(this);
            link.check_status();
        }

        /// <summary>
        /// Sets the tool pose of a robot. The tool pose can be either an item or a 4x4 Matrix.
        /// If "tool" is an item, it links the robot to the tool item. If tool is a 4x4 Matrix, it updates the linked pose of the robot tool.
        /// </summary>
        /// <param name="tool">item/pose -> tool item or 4x4 Matrix (pose of the tool frame)</param>
        public void setTool(Item tool)
        {
            link.check_connection();
            string command = "S_Tool_ptr";
            link.send_line(command);
            link.send_item(tool);
            link.send_item(this);
            link.check_status();
        }

        /// <summary>
        /// Sets the tool pose of a robot. The tool pose can be either an item or a 4x4 Matrix.
        /// If "tool" is an item, it links the robot to the tool item. If tool is a 4x4 Matrix, it updates the linked pose of the robot tool.
        /// </summary>
        /// <param name="tool">item/pose -> tool item or 4x4 Matrix (pose of the tool frame)</param>
        public void setTool(Mat tool)
        {
            link.check_connection();
            string command = "S_Tool";
            link.send_line(command);
            link.send_pose(tool);
            link.send_item(this);
            link.check_status();
        }

        /// <summary>
        /// Adds an empty tool to the robot provided the tool pose (4x4 Matrix) and the tool name.
        /// </summary>
        /// <param name="tool_pose">pose -> TCP as a 4x4 Matrix (pose of the tool frame)</param>
        /// <param name="tool_name">New tool name</param>
        /// <returns>new item created</returns>
        public Item AddTool(Mat tool_pose, string tool_name = "New TCP")
        {
            link.check_connection();
            string command = "AddToolEmpty";
            link.send_line(command);
            link.send_item(this);
            link.send_pose(tool_pose);
            link.send_line(tool_name);
            Item newtool = link.rec_item();
            link.check_status();
            return newtool;
        }

        /// <summary>
        /// Computes the forward kinematics of the robot for the provided joints. The tool and the reference frame are not taken into account.
        /// </summary>
        /// <param name="joints"></param>
        /// <returns>4x4 homogeneous matrix: pose of the robot flange with respect to the robot base</returns>
        public Mat SolveFK(double[] joints)
        {
            link.check_connection();
            string command = "G_FK";
            link.send_line(command);
            link.send_array(joints);
            link.send_item(this);
            Mat pose = link.rec_pose();
            link.check_status();
            return pose;
        }

        /// <summary>
        /// Returns the robot configuration state for a set of robot joints.
        /// </summary>
        /// <param name="joints">array of joints</param>
        /// <returns>3-array -> configuration status as [REAR, LOWERARM, FLIP]</returns>
        public double[] JointsConfig(double[] joints)
        {
            link.check_connection();
            string command = "G_Thetas_Config";
            link.send_line(command);
            link.send_array(joints);
            link.send_item(this);
            double[] config = link.rec_array();
            link.check_status();
            return config;
        }

        /// <summary>
        /// Computes the inverse kinematics for the specified robot and pose. The joints returned are the closest to the current robot configuration (see SolveIK_All())
        /// </summary>
        /// <param name="pose">4x4 matrix -> pose of the robot flange with respect to the robot base frame</param>
        /// <returns>array of joints</returns>
        public double[] SolveIK(Mat pose)
        {
            link.check_connection();
            string command = "G_IK";
            link.send_line(command);
            link.send_pose(pose);
            link.send_item(this);
            double[] joints = link.rec_array();
            link.check_status();
            return joints;
        }

        /// <summary>
        /// Computes the inverse kinematics for the specified robot and pose. The function returns all available joint solutions as a 2D matrix.
        /// </summary>
        /// <param name="pose">4x4 matrix -> pose of the robot tool with respect to the robot frame</param>
        /// <returns>double x n x m -> joint list (2D matrix)</returns>
        public Mat SolveIK_All(Mat pose)
        {
            link.check_connection();
            string command = "G_IK_cmpl";
            link.send_line(command);
            link.send_pose(pose);
            link.send_item(this);
            Mat joints_list = link.rec_matrix();
            link.check_status();
            return joints_list;
        }

        /// <summary>
        /// Connect to a real robot using the robot driver.
        /// </summary>
        /// <param name="robot_ip">IP of the robot to connect. Leave empty to use the one defined in RoboDK</param>
        /// <returns>status -> true if connected successfully, false if connection failed</returns>
        public bool Connect(string robot_ip = "")
        {
            link.check_connection();
            string command = "Connect";
            link.send_line(command);
            link.send_item(this);
            link.send_line(robot_ip);
            int status = link.rec_int();
            link.check_status();
            return status != 0;
        }

        /// <summary>
        /// Disconnect from a real robot (when the robot driver is used)
        /// </summary>
        /// <returns>status -> true if disconnected successfully, false if it failed. It can fail if it was previously disconnected manually for example.</returns>
        public bool Disconnect( )
        {
            link.check_connection();
            string command = "Disconnect";
            link.send_line(command);
            link.send_item(this);
            int status = link.rec_int();
            link.check_status();
            return status != 0;
        }

        /// <summary>
        /// Moves a robot to a specific target ("Move Joint" mode). By default, this function blocks until the robot finishes its movements.
        /// </summary>
        /// <param name="target">target -> target to move to as a target item (RoboDK target item)</param>
        /// <param name="blocking">blocking -> True if we want the instruction to block until the robot finished the movement (default=true)</param>
        public void MoveJ(Item itemtarget, bool blocking = true)
        {
            link.moveX(itemtarget, null, null, this, 1, blocking);
        }

        /// <summary>
        /// Moves a robot to a specific target ("Move Joint" mode). By default, this function blocks until the robot finishes its movements.
        /// </summary>
        /// <param name="target">joints -> joint target to move to.</param>
        /// <param name="blocking">blocking -> True if we want the instruction to block until the robot finished the movement (default=true)</param>
        public void MoveJ(double[] joints, bool blocking = true)
        {
            link.moveX(null, joints, null, this, 1, blocking);
        }

        /// <summary>
        /// Moves a robot to a specific target ("Move Joint" mode). By default, this function blocks until the robot finishes its movements.
        /// </summary>
        /// <param name="target">pose -> pose target to move to. It must be a 4x4 Homogeneous matrix</param>
        /// <param name="blocking">blocking -> True if we want the instruction to block until the robot finished the movement (default=true)</param>
        public void MoveJ(Mat target, bool blocking = true)
        {
            link.moveX(null, null, target, this, 1, blocking);
        }

        /// <summary>
        /// Moves a robot to a specific target ("Move Linear" mode). By default, this function blocks until the robot finishes its movements.
        /// </summary>
        /// <param name="itemtarget">target -> target to move to as a target item (RoboDK target item)</param>
        /// <param name="blocking">blocking -> True if we want the instruction to block until the robot finished the movement (default=true)</param>
        public void MoveL(Item itemtarget, bool blocking = true)
        {
            link.moveX(itemtarget, null, null, this, 2, blocking);
        }

        /// <summary>
        /// Moves a robot to a specific target ("Move Linear" mode). By default, this function blocks until the robot finishes its movements.
        /// </summary>
        /// <param name="joints">joints -> joint target to move to.</param>
        /// <param name="blocking">blocking -> True if we want the instruction to block until the robot finished the movement (default=true)</param>
        public void MoveL(double[] joints, bool blocking = true)
        {
            link.moveX(null, joints, null, this, 2, blocking);
        }

        /// <summary>
        /// Moves a robot to a specific target ("Move Linear" mode). By default, this function blocks until the robot finishes its movements.
        /// </summary>
        /// <param name="target">pose -> pose target to move to. It must be a 4x4 Homogeneous matrix</param>
        /// <param name="blocking">blocking -> True if we want the instruction to block until the robot finished the movement (default=true)</param>
        public void MoveL(Mat target, bool blocking = true)
        {
            link.moveX(null, null, target, this, 2, blocking);
        }

        /// <summary>
        /// Moves a robot to a specific target ("Move Circular" mode). By default, this function blocks until the robot finishes its movements.
        /// </summary>
        /// <param name="itemtarget1">target -> intermediate target to move to as a target item (RoboDK target item)</param>
        /// <param name="itemtarget2">target -> final target to move to as a target item (RoboDK target item)</param>
        /// <param name="blocking">blocking -> True if we want the instruction to block until the robot finished the movement (default=true)</param>
        public void MoveC(Item itemtarget1, Item itemtarget2, bool blocking = true)
        {
            link.moveC_private(itemtarget1, null, null, itemtarget2, null, null, this, blocking);
        }

        /// <summary>
        /// Moves a robot to a specific target ("Move Circular" mode). By default, this function blocks until the robot finishes its movements.
        /// </summary>
        /// <param name="joints1">joints -> intermediate joint target to move to.</param>
        /// <param name="joints2">joints -> final joint target to move to.</param>
        /// <param name="blocking">blocking -> True if we want the instruction to block until the robot finished the movement (default=true)</param>
        public void MoveC(double[] joints1, double[] joints2, bool blocking = true)
        {
            link.moveC_private(null, joints1, null, null, joints2, null, this, blocking);
        }

        /// <summary>
        /// Moves a robot to a specific target ("Move Circular" mode). By default, this function blocks until the robot finishes its movements.
        /// </summary>
        /// <param name="target1">pose -> intermediate pose target to move to. It must be a 4x4 Homogeneous matrix</param>
        /// <param name="target2">pose -> final pose target to move to. It must be a 4x4 Homogeneous matrix</param>
        /// <param name="blocking">blocking -> True if we want the instruction to block until the robot finished the movement (default=true)</param>
        public void MoveC(Mat target1, Mat target2, bool blocking = true)
        {
            link.moveC_private(null, null, target1, null, null, target2, this, blocking);
        }

        /// <summary>
        /// Checks if a joint movement is free of collision.
        /// </summary>
        /// <param name="j1">joints -> start joints</param>
        /// <param name="j2">joints -> destination joints</param>
        /// <param name="minstep_deg">(optional): maximum joint step in degrees</param>
        /// <returns>collision : returns 0 if the movement is free of collision. Otherwise it returns the number of pairs of objects that collided if there was a collision.</returns>
        public int MoveJ_Collision(double[] j1, double[] j2, double minstep_deg = -1)
        {
            link.check_connection();
            string command = "CollisionMove";
            link.send_line(command);
            link.send_item(this);
            link.send_array(j1);
            link.send_array(j2);
            link.send_int((int)(minstep_deg * 1000.0));
            int collision = link.rec_int();
            link.check_status();
            return collision;
        }

        /// <summary>
        /// Checks if a linear movement is free of collision.
        /// </summary>
        /// <param name="j1">joints -> start joints</param>
        /// <param name="j2">joints -> destination joints</param>
        /// <param name="minstep_deg">(optional): maximum joint step in degrees</param>
        /// <returns>collision : returns 0 if the movement is free of collision. Otherwise it returns the number of pairs of objects that collided if there was a collision.</returns>
        public int MoveL_Collision(double[] j1, double[] j2, double minstep_deg = -1)
        {
            link.check_connection();
            string command = "CollisionMoveL";
            link.send_line(command);
            link.send_item(this);
            link.send_array(j1);
            link.send_array(j2);
            link.send_int((int)(minstep_deg * 1000.0));
            int collision = link.rec_int();
            link.check_status();
            return collision;
        }

        /// <summary>
        /// Sets the speed and/or the acceleration of a robot.
        /// </summary>
        /// <param name="speed">speed -> speed in mm/s (-1 = no change)</param>
        /// <param name="accel">acceleration (optional) -> acceleration in mm/s2 (-1 = no change)</param>
        public void setSpeed(double speed, double accel = -1)
        {
            link.check_connection();
            string command = "S_Speed";
            link.send_line(command);
            link.send_int((int)(speed * 1000.0));
            link.send_int((int)(accel * 1000.0));
            link.send_item(this);
            link.check_status();

        }

        /// <summary>
        /// Sets the robot movement smoothing accuracy (also known as zone data value).
        /// </summary>
        /// <param name="zonedata">zonedata value (int) (robot dependent, set to -1 for fine movements)</param>
        public void setZoneData(double zonedata)
        {
            link.check_connection();
            string command = "S_ZoneData";
            link.send_line(command);
            link.send_int((int)(zonedata * 1000.0));
            link.send_item(this);
            link.check_status();
        }

        /// <summary>
        /// Displays a sequence of joints
        /// </summary>
        /// <param name="sequence">joint sequence as a 6xN matrix or instruction sequence as a 7xN matrix</param>
        public void ShowSequence(Mat sequence)
        {
            link.check_connection();
            string command = "Show_Seq";
            link.send_line(command);
            link.send_matrix(sequence);
            link.send_item(this);
            link.check_status();
        }


        /// <summary>
        /// Checks if a robot or program is currently running (busy or moving)
        /// </summary>
        /// <returns>busy status (1=moving, 0=stopped)</returns>
        public int Busy( )
        {
            link.check_connection();
            string command = "IsBusy";
            link.send_line(command);
            link.send_item(this);
            int busy = link.rec_int();
            link.check_status();
            return busy;
        }

        /// <summary>
        /// Stops a program or a robot
        /// </summary>
        /// <returns></returns>
        public void Stop( )
        {
            link.check_connection();
            string command = "Stop";
            link.send_line(command);
            link.send_item(this);
            link.check_status();
        }

        /// <summary>
        /// Waits (blocks) until the robot finishes its movement.
        /// </summary>
        /// <param name="timeout_sec">timeout -> Max time to wait for robot to finish its movement (in seconds)</param>
        public void WaitMove(double timeout_sec = 300)
        {
            link.check_connection();
            string command = "WaitMove";
            link.send_line(command);
            link.send_item(this);
            link.check_status();
            link.COM.ReceiveTimeout = (int)(timeout_sec * 1000.0);
            link.check_status();//will wait here;
            link.COM.ReceiveTimeout = link.TIMEOUT;
            //int isbusy = link.Busy(this);
            //while (isbusy)
            //{
            //    busy = link.Busy(item);
            //}
        }

        ///////// ADD MORE METHODS


        // ---- Program item calls -----

        /// <summary>
        /// Saves a program to a file.
        /// </summary>
        /// <param name="filename">File path of the program</param>
        /// <returns>success</returns>
        public bool MakeProgram(string filename)
        {
            link.check_connection();
            string command = "MakeProg";
            link.send_line(command);
            link.send_item(this);
            link.send_line(filename);
            int prog_status = link.rec_int();
            string prog_log_str = link.rec_line();
            link.check_status();
            bool success = false;
            if (prog_status > 1)
            {
                success = true;
            }
            return success; // prog_log_str
        }

        /// <summary>
        /// Sets if the program will be run in simulation mode or on the real robot.
        /// Use: "PROGRAM_RUN_ON_SIMULATOR" to set the program to run on the simulator only or "PROGRAM_RUN_ON_ROBOT" to force the program to run on the robot.
        /// </summary>
        /// <returns>number of instructions that can be executed</returns>
        public void setRunType(int program_run_type)
        {
            link.check_connection();
            string command = "S_ProgRunType";
            link.send_line(command);
            link.send_item(this);
            link.send_int(program_run_type);
            link.check_status();
        }

        /// <summary>
        /// Runs a program. It returns the number of instructions that can be executed successfully (a quick program check is performed before the program starts)
        /// This is a non-blocking call. Use IsBusy() to check if the program execution finished.
        /// Notes:
        /// if setRunMode(RUNMODE_SIMULATE) is used  -> the program will be simulated (default run mode)
        /// if setRunMode(RUNMODE_RUN_ROBOT) is used -> the program will run on the robot (default when RUNMODE_RUN_ROBOT is used)
        /// if setRunMode(RUNMODE_RUN_ROBOT) is used together with program.setRunType(PROGRAM_RUN_ON_ROBOT) -> the program will run sequentially on the robot the same way as if we right clicked the program and selected "Run on robot" in the RoboDK GUI        
        /// </summary>
        /// <returns>number of instructions that can be executed</returns>
        public int RunProgram( )
        {
            link.check_connection();
            string command = "RunProg";
            link.send_line(command);
            link.send_item(this);
            int prog_status = link.rec_int();
            link.check_status();
            return prog_status;
        }

        /// <summary>
        /// Sets a variable (output) to a given value. This can also be used to set any variables to a desired value.
        /// </summary>
        /// <param name="io_var">io_var -> digital output (string or number)</param>
        /// <param name="io_value">io_value -> value (string or number)</param>
        public void setDO(string io_var, string io_value)
        {
            link.check_connection();
            string command = "setDO";
            link.send_line(command);
            link.send_item(this);
            link.send_line(io_var);
            link.send_line(io_value);
            link.check_status();
        }

        /// <summary>
        /// Waits for an input io_id to attain a given value io_value. Optionally, a timeout can be provided.
        /// </summary>
        /// <param name="io_var">io_var -> digital output (string or number)</param>
        /// <param name="io_value">io_value -> value (string or number)</param>
        /// <param name="timeout_ms">int (optional) -> timeout in miliseconds</param>
        public void waitDI(string io_var, string io_value, double timeout_ms = -1)
        {
            link.check_connection();
            string command = "waitDI";
            link.send_line(command);
            link.send_item(this);
            link.send_line(io_var);
            link.send_line(io_value);
            link.send_int((int)(timeout_ms * 1000.0));
            link.check_status();
        }



        /// <summary>
        /// Adds a new robot move joint instruction to a program.
        /// </summary>
        /// <param name="itemtarget">target to move to</param>
        public void addMoveJ(Item itemtarget)
        {
            link.check_connection();
            string command = "Add_INSMOVE";
            link.send_line(command);
            link.send_item(itemtarget);
            link.send_item(this);
            link.send_int(1);
            link.check_status();
        }

        /// <summary>
        /// Adds a new robot move linear instruction to a program.
        /// </summary>
        /// <param name="itemtarget">target to move to</param>
        public void addMoveL(Item itemtarget)
        {
            link.check_connection();
            string command = "Add_INSMOVE";
            link.send_line(command);
            link.send_item(itemtarget);
            link.send_item(this);
            link.send_int(2);
            link.check_status();
        }

        ////////// ADD MORE METHODS
        /// <summary>
        /// Returns the number of instructions of a program.
        /// </summary>
        /// <returns></returns>
        public int InstructionCount( )
        {
            link.check_connection();
            string command = "Prog_Nins";
            link.send_line(command);
            link.send_item(this);
            int nins = link.rec_int();
            link.check_status();
            return nins;
        }

        /// <summary>
        /// Returns the program instruction at position id
        /// </summary>
        /// <param name="ins_id"></param>
        /// <param name="name"></param>
        /// <param name="instype"></param>
        /// <param name="movetype"></param>
        /// <param name="isjointtarget"></param>
        /// <param name="target"></param>
        /// <param name="joints"></param>
        public void Instruction(int ins_id, out string name, out int instype, out int movetype, out bool isjointtarget, out Mat target, out double[] joints)
        {
            link.check_connection();
            string command = "Prog_GIns";
            link.send_line(command);
            link.send_item(this);
            link.send_int(ins_id);
            name = link.rec_line();
            instype = link.rec_int();
            movetype = 0;
            isjointtarget = false;
            target = null;
            joints = null;
            if (instype == INS_TYPE_MOVE)
            {
                movetype = link.rec_int();
                isjointtarget = link.rec_int() > 0 ? true : false;
                target = link.rec_pose();
                joints = link.rec_array();
            }
            link.check_status();
        }

        /// <summary>
        /// Sets the program instruction at position id
        /// </summary>
        /// <param name="ins_id"></param>
        /// <param name="name"></param>
        /// <param name="instype"></param>
        /// <param name="movetype"></param>
        /// <param name="isjointtarget"></param>
        /// <param name="target"></param>
        /// <param name="joints"></param>
        public void setInstruction(int ins_id, string name, int instype, int movetype, bool isjointtarget, Mat target, double[] joints)
        {
            link.check_connection();
            string command = "Prog_SIns";
            link.send_line(command);
            link.send_item(this);
            link.send_int(ins_id);
            link.send_line(name);
            link.send_int(instype);
            if (instype == INS_TYPE_MOVE)
            {
                link.send_int(movetype);
                link.send_int(isjointtarget ? 1 : 0);
                link.send_pose(target);
                link.send_array(joints);
            }
            link.check_status();
        }


        /// <summary>
        /// Returns the list of program instructions as an MxN matrix, where N is the number of instructions and M equals to 1 plus the number of robot axes.
        /// </summary>
        /// <param name="instructions">the matrix of instructions</param>
        /// <returns>Returns 0 if success</returns>
        public int InstructionList(out Mat instructions)
        {
            link.check_connection();
            string command = "G_ProgInsList";
            link.send_line(command);
            link.send_item(this);
            instructions = link.rec_matrix();
            int errors = link.rec_int();
            link.check_status();
            return errors;
        }

        /// <summary>
        /// Returns a list of joints an MxN matrix, where M is the number of robot axes plus 4 columns. Linear moves are rounded according to the smoothing parameter set inside the program.
        /// </summary>
        /// <param name="error_msg">Returns a human readable error message (if any)</param>
        /// <param name="joint_list">Returns the list of joints as [J1, J2, ..., Jn, ERROR, MM_STEP, DEG_STEP, MOVE_ID] if a file name is not specified</param>
        /// <param name="mm_step">Maximum step in millimeters for linear movements (millimeters)</param>
        /// <param name="deg_step">Maximum step for joint movements (degrees)</param>
        /// <param name="save_to_file">Provide a file name to directly save the output to a file. If the file name is not provided it will return the matrix. If step values are very small, the returned matrix can be very large.</param>
        /// <returns>Returns 0 if success, otherwise, it will return negative values</returns>
        public int InstructionListJoints(out string error_msg, out Mat joint_list, double mm_step = 10.0, double deg_step = 5.0, string save_to_file = "")
        {
            link.check_connection();
            string command = "G_ProgJointList";
            link.send_line(command);
            link.send_item(this);
            double[] ste_mm_deg = { mm_step, deg_step };
            link.send_array(ste_mm_deg);
            //joint_list = save_to_file;
            if (save_to_file.Length <= 0)
            {
                link.send_line("");
                joint_list = link.rec_matrix();
            }
            else
            {
                link.send_line(save_to_file);
                joint_list = null;
            }
            int error_code = link.rec_int();
            error_msg = link.rec_line();
            link.check_status();
            return error_code;
        }
    }

}

