using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SlimDX;

namespace SlimFrame.Cameras
{
	/// <summary>
	/// The Base for various cameras.
	/// </summary>
	public class Camera
	{

		#region Member Variables

		protected Matrix m_viewMatrix;			//The matrix representing the view transformation.
		protected Matrix m_viewReflectMatrix;	//The matrix representing the reflected view transformation.
		protected Matrix m_Transformation;		//The matrix representing the transformation for camera vectors.

		protected Vector3 m_Position;	//The position of camera. 
		protected Vector3 m_oPosition;	//The old position of camera. 
		protected Vector3 m_LookAt;		//The position where camera is looking.
		protected Vector3 m_Look;		//The position where camera is looking.
		protected Vector3 m_Up;			//The up direction of camera.
		protected Vector3 m_Left;		//The left direction of camera.

		protected Vector3 m_rot;        //The rotation components of camera along x-axis, y-axis & z-axis respectively

		#endregion

		#region Properties

		/// <summary>
		/// The position of the Camera.
		/// </summary>
		public Vector3 Position
		{
			get
			{
				return m_Position;
			}
			set
			{
				m_oPosition = m_Position;
				m_Position = value;
			}
		}
		
		/// <summary>
		/// The rotation of camera about X-axis.(in Radian)
		/// </summary>
		public float Pitch
		{
			get
			{
				return m_rot.X;
			}
			set
			{
				m_rot.X = (float)Math.Min(Math.Max(value, -89.9f / 180.0f * Math.PI), 89.9f / 180.0f * Math.PI);
			}
		}

		/// <summary>
		/// The rotation of camera about Y-axis.(in Radian)
		/// </summary>
		public float Yaw
		{
			get
			{
				return m_rot.Y;
			}
			set
			{
				m_rot.Y = value;
			}
		}

		/// <summary>
		/// The rotation of camera about Z-axis.(in Radian)
		/// </summary>
		public float Roll
		{
			get
			{
				return m_rot.Z;
			}
			set
			{
				m_rot.Z = value;
			}
		}

		/// <summary>
		/// Gives the position of 1st person Camera with some portion of look direction added to give some realastic feel while rotating the Camera 
		/// this specified Position shows that Camera is box/ball with lens at a specific position on surface rather than just a point Object Camera. 
		/// </summary>
		public Vector3 PositionSpecial
		{
			get
			{
				return m_Position + m_LookAt;
			}
		}

		/// <summary>
		/// The difference in camera position from the last frame to current frame
		/// It Can be used for finding camera movement speed
		/// </summary>
		public Vector3 PositionDelta
		{
			get
			{
				return m_Position - m_oPosition;
			}
		}

		/// <summary>
		/// The vector pointing in the up direction of Camera.
		/// </summary>
		public Vector3 Up
		{
			get
			{
				return Vector3.Normalize(m_Up);
			}
		}

		/// <summary>
		/// The vector pointing in the front direction of Camera.
		/// </summary>
		public Vector3 Front
		{
			get
			{
				return Vector3.Normalize(m_Look);
			}
		}

		/// <summary>
		/// The vector pointing in the back direction of Camera.
		/// </summary>
		public Vector3 Back
		{
			get
			{
				return -Front;
			}
		}

		/// <summary>
		/// The vector pointing in the right direction of Camera.
		/// </summary>
		public Vector3 Right
		{
			get
			{
				Vector3 right = Vector3.Cross(m_Up, m_LookAt);
				right.Normalize();
				return right;
			}
		}

		/// <summary>
		/// The vector pointing in the left direction of Camera.
		/// </summary>
		public Vector3 Left
		{
			get
			{
				return -Right;
			}
		}

		/// <summary>
		/// The vector pointing in the down direction of Camera.
		/// </summary>
		public Vector3 Down
		{
			get
			{
				return -Up;
			}
		}

		/// <summary>
		/// View transformation matrix of the Camera.
		/// </summary>
		public Matrix ViewMatrix
		{
			get
			{
				return m_viewMatrix;
			}
		}

		/// <summary>
		/// Reflection view transformation matrix of the Camera.
		/// </summary>
		public Matrix ReflectionViewMatrix
		{
			get
			{
				return m_viewReflectMatrix;
			}
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Creates new Camera instance using default value
		/// </summary>
		public Camera()
		{
			m_Position = new Vector3(0,0,-1);
			m_Look = -m_Position;
			m_Look.Normalize();
			m_Up = new Vector3(0, 1, 0);
		}

		/// <summary>
		/// Creates a new Camera instance with no rotation.
		/// </summary>
		/// <param name="pos">The position of camera</param>
		/// <param name="target">The target of camera</param>
		/// <param name="up">The up direction of camera</param>
		public Camera(Vector3 pos, Vector3 target, Vector3 up)
		{
			m_Position = pos;
			m_Look = target - pos;
			m_Look.Normalize();
			m_Up = up;
		}

		/// <summary>
		/// Creates a new Camera instance.
		/// </summary>
		/// <param name="pos">Position of camera</param>
		/// <param name="target">Target of camera</param>
		/// <param name="up">Up direction of camera</param>
		/// <param name="pitch">Rotation of camera about X-axis (in Radian)</param>
		/// <param name="yaw">Rotation of camera about Y-axis (in Radian)</param>
		/// <param name="roll">Rotation of camera about Z-axis (in Radian)</param>
		public Camera(Vector3 pos, Vector3 target, Vector3 up, float pitch, float yaw, float roll)
		{
			m_Position = pos;
			m_Look = target - pos;
			m_Look.Normalize();
			m_Up = up;
			m_rot = new Vector3(pitch, yaw, roll);
		}

		#endregion

		#region Methods

		/// <summary>
		/// Builds view transformation matrix for producing reflection effect for planar Object
		/// </summary>
		/// <param name="height">The height of the floor.</param>
		public virtual void RenderReflection(float floorHeight)
		{

		}

		/// <summary>
		/// Updates Camera's rotation transformation.
		/// </summary>
		public virtual void Update()
		{
			m_Transformation = Matrix.RotationYawPitchRoll(m_rot.Y, m_rot.X, m_rot.Z);
		}

		#endregion

	}

}
