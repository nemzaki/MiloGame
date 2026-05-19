module.exports = async ({ params, context, logger }) => {
    const latestVersion = "0.0.5"; // Change this to your latest version
    const clientVersion = params.version;

    if (!clientVersion) {
        return { error: "MISSING_VERSION" };
    }

    if (clientVersion !== latestVersion) {
        return { error: "OUTDATED_VERSION" };
    }

    return { message: "Version is up to date!" };
};

// Define expected parameters
module.exports.params = {
    "version": "string"
};