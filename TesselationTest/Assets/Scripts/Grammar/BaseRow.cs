using UnityEngine;

namespace Demo {
	public class BaseRow : Shape {
		int Number;
		GameObject[] prefabs=null;
		GameObject[] doorPrefabs=null;
		Vector3 direction;

		public void Initialize(int Number, GameObject[] prefabs, GameObject[] doorPrefabs = null, Vector3 dir=new Vector3()) {
			this.Number=Number;
			this.prefabs=prefabs;
			this.doorPrefabs=doorPrefabs;
			if (dir.magnitude!=0) {
				direction=dir;
			} else {
				direction=new Vector3(0, 0, 1);
			}
		}

		protected override void Execute() {
			if (Number<=0)
				return;

			int doorIndex = RandomInt(Number);  //choose where the door will be

			for (int i=0;i<Number;i++) {    // spawn the prefabs, randomly chosen
				int index;

				if (doorPrefabs != null &&  doorIndex == i)
				{
					index = RandomInt(doorPrefabs.Length);

					SpawnPrefab(doorPrefabs[index],
					direction * (i - (Number - 1) / 2f),
					Quaternion.identity         
					);
				}
				else
				{
					index = RandomInt(prefabs.Length); // choose a random prefab index

					SpawnPrefab(prefabs[index],
						direction * (i - (Number - 1) / 2f), // position offset from center
						Quaternion.identity         // no rotation
					);
				}
			}

		}
	}
}
