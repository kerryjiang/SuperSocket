// Create the pipeline filter and encoder
var pipelineFilter = new ProtobufPipelineFilter();
var encoder = new ProtobufPackageEncoder();

// Register message types with their IDs
pipelineFilter.RegisterMessageType(1, YourFirstMessageType.Parser, typeof(YourFirstMessageType));
pipelineFilter.RegisterMessageType(2, YourSecondMessageType.Parser, typeof(YourSecondMessageType));

// Register the same types for encoding
encoder.RegisterMessageType(typeof(YourFirstMessageType), 1);
encoder.RegisterMessageType(typeof(YourSecondMessageType), 2);

// Now you can use them in your SuperSocket server configuration
var server = SuperSocketHostBuilder.Create<ProtobufPackageInfo>()
    .UsePipelineFilter(pipelineFilter)
    .UsePackageEncoder(encoder)
    .UsePackageHandler(async (session, package) =>
    {
        // Handle different message types based on package.TypeId or package.MessageType
        if (package.TypeId == 1)
        {
            var message = (YourFirstMessageType)package.Message;
            // Handle first message type
        }
        else if (package.TypeId == 2)
        {
            var message = (YourSecondMessageType)package.Message;
            // Handle second message type
        }
        
        // Send a response
        await session.SendAsync(new ProtobufPackageInfo
        {
            Message = yourResponseMessage,
            TypeId = responseTypeId
        });
    })
    .BuildAsServer();