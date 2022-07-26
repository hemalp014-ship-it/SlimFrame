using SlimDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlimFrame.Cameras
{

	/// <summary>
	/// The Third Person Free look Camera
	/// </summary>
	public class ThirdPersonCamera : Camera
	{
		#region Member Variables

		private float l; //The distance of camera from the camera origin.
		private Vector3 m_Origin; //The origin of camera.

		#endregion

		#region Properties

		/// <summary>
		/// The origin of camera.
		/// </summary>
		public Vector3 Origin
		{
			get
			{
				return m_Origin;
			}
			set
			{
				m_Origin = value;
			}
		}

		/// <summary>
		/// The distance of camera from the camera origin.
		/// </summary>
		public float Distance
		{
			get
			{
				return l;
			}
			set
			{
				l = Math.Max(value, 1.0e-5f);
			}
		}

		#endregion

		#region Contructors

		/// <summary>
		/// Creates a Third Person Camera instance with no rotation specified.
		/// </summary>
		/// <param name="origin">Pivot point of the Third Person Camera</param>
		/// <param name="dist">Distance of Third Person Camera from pivot point</param>
		/// <param name="up">The up direction of Third Person Camera</param>
		public ThirdPersonCamera(Vector3 origin, float dist, Vector3 up)
		{
			m_Up = up;
			m_Look = -new Vector3(0, 0, 1);
			m_Look.Normalize();

			m_LookAt = m_Look;
			m_Origin = origin;
			l = dist;
		}

		/// <summary>
		/// Creates a Third Person Camera instance.
		/// </summary>
		/// <param name="origin">Pivot point of the Third Person Camera</param>
		/// <param name="dist">Distance of Third Person Camera from pivot point</param>
		/// <param name="up">The up direction of Third Person Camera</param>
		/// <param name="pitch">Rotation of camera about X-axis (in Radian)</param>
		/// <param name="yaw">Rotation of camera about Y-axis (in Radian)</param>
		/// <param name="roll">Rotation of camera about Z-axis (in Radian)</param>
		public ThirdPersonCamera(Vector3 origin, float dist, Vector3 up, float pitch, float yaw, float roll)
		{
			m_Up = up;
			m_Look = -new Vector3(0, 0, 1);
			m_Look.Normalize();

			m_LookAt = m_Look;
			m_Origin = origin;
			l = dist;

			m_rot = new Vector3(pitch, yaw, roll);
		}

		#endregion

		#region Methods

		/// <summary>
		/// Updates Third Person Camera state.
		/// </summary>
		public override void Update()
		{
			base.Update();
			Vector3 look = m_Look;

			m_LookAt = -(m_Origin - Position);
			m_LookAt.Normalize();

			look = Vector3.TransformCoordinate(look, m_Transformation);
			look.Normalize();

			m_Position = -look * l + m_Origin;

			m_viewMatrix = Matrix.LookAtLH(m_Position, m_Origin, m_Up);
		}

		/// <summary>
		/// Renders Reflection Matrix for Third Person Camera
		/// </summary>
		public override void RenderReflection(float floorHeight)
		{
			//Vector3 pos, lookAt;

			//Pitch = -Pitch;
			//Update();
			//Pitch = -Pitch;

			//pos = Position; lookAt = m_LookAt;

			//// Setup the position of the FPCamera in the world.
			//// For planar reflection invert the Y position of the FPCamera.
			//pos.Y = -pos.Y + (floorHeight * 2.0f);

			//m_viewReflectMatrix = Matrix.LookAtLH(pos, pos + lookAt, m_Up);

			//Update();
		}

		#endregion
	}

}
