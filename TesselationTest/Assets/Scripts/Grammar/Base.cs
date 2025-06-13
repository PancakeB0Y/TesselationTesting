using UnityEngine;

namespace Demo {
	public class Base : Shape {
		// grammar rule probabilities:
		const float stockContinueChance = 0.5f;

		// shape parameters:
		[SerializeField]
		public int Width;
		[SerializeField]
        public int Depth;
        [SerializeField]
        public int MinHeight;
        [SerializeField]
        public int MaxHeight;

		int CurrentHeight = 1;

        [SerializeField]
		GameObject[] wallStyle;
		[SerializeField]
		GameObject[] roofStyle;
        [SerializeField]
        GameObject[] doorStyle;

        public void Initialize(int Width, int Depth, GameObject[] wallStyle, GameObject[] roofStyle) {
			this.Width=Width;
			this.Depth=Depth;
			this.wallStyle=wallStyle;
			this.roofStyle=roofStyle;
		}

		protected override void Execute() {
			// Create four walls:

			GameObject[] curDoorStyle = null;

            for (int i = 0; i<4; i++) {
				Vector3 localPosition = new Vector3();
				switch (i) {
					case 0:
						localPosition = new Vector3(-(Width-1)*0.5f, 0, 0); // left
						break;
					case 1:
						localPosition = new Vector3(0, 0, (Depth-1)*0.5f); // back
						break;
					case 2:
						localPosition = new Vector3((Width-1)*0.5f, 0, 0); // right
						break;
					case 3:
						localPosition = new Vector3(0, 0, -(Depth-1)*0.5f); // front
						curDoorStyle = doorStyle;
                        break;
				}
				BaseRow newRow = CreateSymbol<BaseRow>("wall", localPosition, Quaternion.Euler(0, i*90, 0));
				newRow.Initialize(i%2==1 ? Width : Depth, wallStyle, curDoorStyle);
				newRow.Generate();
			}		

			// Continue with a floor or with a roof (depending on max height):
			if (CurrentHeight < MaxHeight) {
				SimpleFloor nextFloor = CreateSymbol<SimpleFloor>("stock", new Vector3(0, 1, 0));
				nextFloor.Initialize(Width, Depth, MinHeight, MaxHeight, CurrentHeight + 1, wallStyle, roofStyle);
				nextFloor.Generate(buildDelay);
			} else {
                RoofStretch nextRoof = CreateSymbol<RoofStretch>("roof", new Vector3(0, 1, 0));
				nextRoof.Initialize(Width, Depth, MinHeight, MaxHeight, CurrentHeight + 1, roofStyle,wallStyle);
				nextRoof.Generate(buildDelay);
			}
		}
	}
}
