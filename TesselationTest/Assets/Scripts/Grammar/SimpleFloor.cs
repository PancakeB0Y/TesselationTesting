using UnityEngine;

namespace Demo {
	public class SimpleFloor : Shape {
		// grammar rule probabilities:
		const float stockContinueChance = 0.5f;

		// shape parameters:
		[SerializeField]
		int Width;
		[SerializeField]
		int Depth;
        [SerializeField]
        int MinHeight;
        [SerializeField]
        int MaxHeight;

        int CurrentHeight = 1;

        [SerializeField]
		GameObject[] wallStyle;
		[SerializeField]
		GameObject[] roofStyle;

		public void Initialize(int Width, int Depth, int MinHeight, int MaxHeight, int CurrentHeight, GameObject[] wallStyle, GameObject[] roofStyle) {
			this.Width=Width;
			this.Depth=Depth;
			this.wallStyle=wallStyle;
			this.roofStyle=roofStyle;
			this.MinHeight = MinHeight;
			this.MaxHeight = MaxHeight;
			this.CurrentHeight = CurrentHeight;
		}

		protected override void Execute() {
			// Create four walls:
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
						break;
				}
				SimpleRow newRow = CreateSymbol<SimpleRow>("wall", localPosition, Quaternion.Euler(0, i*90, 0));
				newRow.Initialize(i%2==1 ? Width : Depth,wallStyle);
				newRow.Generate();
			}

            if (CurrentHeight >= MaxHeight)
            {
                GenerateRoof();
                return;
            }

            if (CurrentHeight < MinHeight)
			{
				GenerateNextFloor();
				return;

			}

            // Continue with a floor or with a roof (random choice):
            float randomValue = RandomFloat();
            if (randomValue < stockContinueChance)
            {
                GenerateNextFloor();
            }
            else
            {
                GenerateRoof();
            }
            
		}

		private void GenerateNextFloor()
		{
            SimpleFloor nextFloor = CreateSymbol<SimpleFloor>("stock", new Vector3(0, 1, 0));
            nextFloor.Initialize(Width, Depth, MinHeight, MaxHeight, CurrentHeight + 1, wallStyle, roofStyle);
            nextFloor.Generate(buildDelay);
        }

        private void GenerateRoof()
        {
            RoofStretch nextRoof = CreateSymbol<RoofStretch>("roof", new Vector3(0, 1, 0));
            nextRoof.Initialize(Width, Depth, MinHeight, MaxHeight, CurrentHeight, roofStyle, wallStyle);
            nextRoof.Generate(buildDelay);
        }
    }
}
