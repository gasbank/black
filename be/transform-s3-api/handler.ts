import { APIGatewayProxyHandler } from "aws-lambda";
import { S3 } from "aws-sdk";
// import "source-map-support/register";

const s3 = new S3();
const bucketName = process.env.SOURCE_BUCKET!;
const inputKeyPrefix = process.env.INPUT_KEY_PREFIX!;
const transformKeyPrefix = process.env.TRANSFORM_KEY_PREFIX!;
const cdnUrlPrefix = process.env.CDN_URL_PREFIX!;
const outputExtensions = [".bytes", "-OTB-FSNB.png", "-OTB-FSNB-BB-SDF.png"];

export const put: APIGatewayProxyHandler = async event => {
  const { key } = event.pathParameters;
  try {
    const signedUrl = s3.getSignedUrl("putObject", {
      Bucket: bucketName,
      Key: inputKeyPrefix + key + ".png",
      Expires: 5 * 60
    });
    return { statusCode: 200, body: signedUrl };
  } catch (error) {
    console.error(`PutError`, key, error);
    return { statusCode: 500, body: error.message };
  }
};

export const get: APIGatewayProxyHandler = async event => {
  const { key } = event.pathParameters;
  try {
    const files: string[] = [];
    for (const ext of outputExtensions) {
      const targetKey = transformKeyPrefix + key + ext;
      console.log(targetKey);

      const head = await s3
        .headObject({
          Bucket: bucketName,
          Key: targetKey
        })
        .promise();
      console.log(head);

      files.push(cdnUrlPrefix + targetKey);
    }
    console.log(files);
    return { statusCode: 200, body: JSON.stringify(files) };
  } catch (error) {
    console.error(`GetError`, key, error);
    return { statusCode: 404, body: error.message };
  }
};

export const erase: APIGatewayProxyHandler = async event => {
  const { key } = event.pathParameters;
  try {
    const keys = [
      inputKeyPrefix + key + ".png",
      ...outputExtensions.map(ext => transformKeyPrefix + key + ext)
    ];
    await Promise.all(
      keys.map(each =>
        s3
          .deleteObject({
            Bucket: bucketName,
            Key: each
          })
          .promise()
      )
    );
    return { statusCode: 200, body: "true" };
  } catch (error) {
    console.error(`DeleteError`, key, error);
    return { statusCode: 500, body: "false" };
  }
};
