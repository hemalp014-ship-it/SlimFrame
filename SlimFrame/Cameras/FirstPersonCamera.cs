using SlimDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlimFrame.Cameras
{
	/// <summary>
	/// The First Person Free look Camera
	/// </summary>
	public class FirstPersonCamera : Camera
	{

		#region Constuctor

		/// <summary>
		/// Creates a First Person Camera instance with no rotation specified.
		/// </summary>
		/// <param name="pos">The position of First Person Camera</param>
		/// <param name="target">The target of First Person Camera</param>
		/// <param name="up">The up direction of First Person Camera</param>
		public FirstPersonCamera(Vector3 pos, Vector3 target, Vector3 up)
		{
			m_Position = pos;
			m_Look = target - pos;
			m_Look.Normalize();
			m_Up = up;
		}

		/// <summary>
		/// Creates a First Person Camera instance.
		/// </summary>
		/// <param name="pos">The position of camera</param>
		/// <param name="target">The target of camera</param>
		/// <param name="up">The up direction of camera</param>
		/// <param name="pitch">Rotation of camera about X-axis (in Radian)</param>
		/// <param name="yaw">Rotation of camera about Y-axis (in Radian)</param>
		/// <param name="roll">Rotation of camera about Z-axis (in Radian)</param>
		public FirstPersonCamera(Vector3 pos, Vector3 target, Vector3 up, float pitch, float yaw, float roll)
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
		/// Updates First Person Camera state.
		/// </summary>
		public override void Update()
		{
			base.Update();

			m_LookAt = Vector3.TransformCoordinate(m_Look, m_Transformation);
			m_LookAt.Normalize();

			m_viewMatrix = Matrix.LookAtLH(PositionSpecial, PositionSpecial + m_LookAt, m_Up);
		}

		/// <summary>
		/// Renders Reflection Matrix for First Person Camera
		/// </summary>
		public override void RenderReflection(float floorHeight)
		{
			Vector3 pos, lookAt;

			Pitch = -Pitch;
			Update();
			Pitch = -Pitch;

			pos = Position; lookAt = m_LookAt;

			// Setup the position of the FPCamera in the world.
			// For planar reflection invert the Y position of the FPCamera.
			pos.Y = -pos.Y + (floorHeight * 2.0f);

			m_viewReflectMatrix = Matrix.LookAtLH(pos, pos + lookAt, m_Up);

			Update();
		}

		#endregion

	}

}
