using System;
using Unity.VisualScripting;
using UnityEngine;

namespace Demo {
	public class SimpleRoof : Shape {
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
			SimpleRow flatRoof;

			switch (side) {
				// Add two roof strips in depth direction
				case 0:
					for (int i = 0; i < 2; i++)
					{
						flatRoof = CreateSymbol<SimpleRow>("roofStrip", new Vector3((Width - 1) * (i - 0.5f), 0, 0));
						flatRoof.Initialize(Depth, roofStyle);
						flatRoof.Generate();
					}

					newWidth -=2;
                    break;
				// Add two roof strips in width direction
				case 1:
					for (int i = 0; i < 2; i++)
					{
						flatRoof = CreateSymbol<SimpleRow>("roofStrip", new Vector3(0, 0, (Depth - 1) * (i - 0.5f)));
						flatRoof.Initialize(Width, roofStyle, new Vector3(1, 0, 0));
						flatRoof.Generate();
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
            SimpleRoof nextRoof = CreateSymbol<SimpleRoof>("roof");
            nextRoof.Initialize(newWidth, newDepth, MinHeight, MaxHeight, CurrentHeight, roofStyle, wallStyle);
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