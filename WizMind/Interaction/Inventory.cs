using WizMind.LuigiAi;

namespace WizMind.Interaction
{
    public class Inventory(Input input, LuigiAiData luigiAiData)
    {
        private readonly Input input = input;
        private readonly LuigiAiData luigiAiData = luigiAiData;

        /// <summary>
        /// Drops the item at the specified slot on the ground.
        /// </summary>
        /// <param name="slot">The slot to drop a part from, from 0-9.</param>
        public void DropItem(int slot)
        {
            if (slot < 0 || slot > 9)
            {
                throw new ArgumentException("Invalid slot");
            }

            var key = slot == 0 ? Keys.D0 : Keys.D1 + slot - 1;
            this.input.SendKeystroke(key, KeyModifier.Alt);
            this.luigiAiData.InvalidateData(DataInvalidationType.AdvancingAction);
        }
    }
}
