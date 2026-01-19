using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using UnityEngine;

namespace Assets.Scripting.PipelineScripting.Utilitary
{
	// Not yet working
	[Serializable]
	public class CustomRotationField
	{
		// stores an equivalent of Vector3 but with convenient modification to facilitate test and debugg of rotations
		[OnValueChanged(nameof(UpdateDirection))][OdinSerialize] public Vector3 direction;
		
		public Vector3 Value {
			get { 
				return direction; 
			}
        }

        private Vector3 lastDirectionValue = Vector3.zero;
		void UpdateDirection()
		{
			direction = new Vector3(Math.Clamp(direction.x, -1, 1), Math.Clamp(direction.y, -1, 1), Math.Clamp(direction.z, -1, 1));
		}
    }
}