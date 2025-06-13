using System;
using Unity.VisualScripting;
using UnityEngine;

namespace Demo {
	public class RoofStretch : Shape {
		// grammar rule probabilities:
		const float roofContinueChance = 0.5f;

		// shape parameters:
		int Width;
		int Depth;
        int MinHeight;
        int MaxHeight;

        int CurrentHeight = 1;

        GameObject[] roofStyle;
		GameObject[] wallStyle;

		// (offset) values for the next layer:
		int newWidth;
		int newDepth;

		public void Initialize(int Width, int Depth, int MinHeight, int MaxHeight, int CurrentHeight, GameObject[] roofStyle, GameObject[] wallStyle) {
			this.Width=Width;
			this.Depth=Depth;
			this.roofStyle=roofStyle;
			this.wallStyle=wallStyle;
			this.MinHeight = MinHeight;
			this.MaxHeight = MaxHeight;
			this.CurrentHeight = CurrentHeight;
		}


		protected override void Execute() {
			if (Width==0 || Depth==0)
				return;

			newWidth=Width;
			newDepth=Depth;

			CreateFlatRoofPart();
			CreateNextPart();
		}

		void CreateFlatRoofPart() {
			// Randomly create two roof strips in depth direction or in width direction:
			int side = RandomInt(2);
			//SimpleRow flatRoof;

			switch (side) {
				// Add two roof strips in depth direction
				case 0:
					for (int i = 0; i < 2; i++)
					{
						int index = RandomInt(roofStyle.Length);
                        GameObject stretchedRoof = SpawnPrefab(roofStyle[index], new Vector3((Width - 1) * (i - 0.5f), 0, 0));
                        stretchedRoof.transform.localScale = new Vector3(1, 1, Depth);
                    }

					newWidth -=2;
                    break;
				// Add two roof strips in width direction
				case 1:
					for (int i = 0; i < 2; i++)
					{
						int index = RandomInt(roofStyle.Length);
						GameObject stretchedRoof = SpawnPrefab(roofStyle[index], new Vector3(0, 0, (Depth - 1) * (i - 0.5f)));
                        stretchedRoof.transform.localScale = new Vector3(Width, 1, 1);

                    }

					newDepth -=2;
                    break;
			}
		}

		void CreateNextPart() {
			// randomly continue with a roof or a stock:
			if (newWidth<=0 || newDepth<=0)
				return;

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
            if (randomValue < roofContinueChance)
            {
                GenerateNextFloor();
            }
            else
            {
                GenerateRoof();
            }
            
		}

		private void GenerateRoof()
		{
            RoofStretch nextRoof = CreateSymbol<RoofStretch>("roof");
            nextRoof.Initialize(newWidth, newDepth, MinHeight, MaxHeight, CurrentHeight + 1, roofStyle, wallStyle);
            nextRoof.Generate(buildDelay);
        }

		private void GenerateNextFloor()
		{
            SimpleFloor nextFloor = CreateSymbol<SimpleFloor>("stock");
            nextFloor.Initialize(newWidth, newDepth, MinHeight, MaxHeight, CurrentHeight + 1, wallStyle, roofStyle);
            nextFloor.Generate(buildDelay);
        }
    }
}