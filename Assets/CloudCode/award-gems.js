// award-gems.js — Unity Cloud Code module
// Deployed to: Unity Dashboard > Cloud Code > Scripts
// Called by: IAPManager.cs (Task 1.4) after a successful Apple/Google receipt
// Args: { receiptData: string, productId: string }
//
// DO NOT deploy this stub until Task 1.4 — receipt validation is not yet implemented.

const { DataApi } = require("@unity-services/cloud-save-data-1.0");

module.exports = async ({ params, context, logger }) => {
  const { receiptData, productId } = params;

  // TODO (Task 1.4): validate receiptData with Apple/Google servers here.
  // Reject if receipt is invalid or already claimed (use an idempotency key stored in Cloud Save).

  const gemAmounts = {
    "gems_small":   100,
    "gems_medium":  550,
    "gems_large":  1200,
  };

  const amount = gemAmounts[productId];
  if (!amount) throw new Error(`Unknown productId: ${productId}`);

  const dataApi = new DataApi(context);
  const current = await dataApi.getItems(context.playerId, ["gems"]);
  const currentGems = current?.data?.gems ?? 0;

  await dataApi.setItem(context.playerId, "gems", currentGems + amount);

  logger.info(`Awarded ${amount} gems to ${context.playerId} (product: ${productId})`);
  return { newBalance: currentGems + amount };
};
