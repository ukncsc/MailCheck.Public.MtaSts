GRANT SELECT, INSERT, UPDATE, DELETE ON `mtasts_entity` TO '{env}-mtasts-ent' IDENTIFIED BY '{password}';
GRANT SELECT ON `mtasts_entity` TO '{env}-mtasts-api' IDENTIFIED BY '{password}';
GRANT SELECT ON `mtasts_entity` TO '{env}_reports' IDENTIFIED BY '{password}';
GRANT SELECT INTO S3 ON *.* TO '{env}_reports' IDENTIFIED BY '{password}';